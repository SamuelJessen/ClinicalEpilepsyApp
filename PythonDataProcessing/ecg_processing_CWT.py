import paho.mqtt.client as paho  # pip install paho-mqtt
from paho import mqtt
from QRSDetector import QRSDetector  # pip install QRSDetector
from hrvanalysis import *  # pip install hrvanalysis
import scipy.signal as signal  # pip install scipy
import numpy as np  # pip install numpy
import json  # pip install json
import datetime  # pip install datetime
import pywt  # pip install pywt
import os

# region Step 0: Setup the MQTT client
Topic_Decoded_Measurement = "ecg_data_group1/decoded_measurements"
Topic_Processed_Measurement = "ecg_data_group1/processed_measurements"


def on_connect(client, userdata, flags, rc, properties=None):
    client.subscribe(Topic_Decoded_Measurement, qos=1)

    print("Connected with result code " + str(rc))


def on_message(client, userdata, msg):
    # Decode the message payload and check the topic
    messageEncoded = msg.payload
    message = messageEncoded.decode("utf-8")

    if msg.topic == Topic_Decoded_Measurement:  # Handle the raw data
        ProcessingAlgorihtm(message)
    else:
        print("Unknown topic: " + msg.topic + "\n\t" + str(message))


def on_disconnect(client, userdata, rc, properties=None):
    print("Disconnected with result code " + str(rc))


def on_subscribe(client, userdata, mid, granted_qos, properties=None):
    print("Subscribed: " + str(mid) + " " + str(granted_qos))


def on_publish(client, userdata, mid):
    print("Message published successfully with MID:", mid)


def on_publish_error(client, userdata, mid):
    print("Error publishing message with MID:", mid)


# endregion


# region Step 1: Deserialize the json string
def DeserializeJson(ms):
    ecgObject = json.loads(ms)

    return ecgObject


# endregion


# region Step 2: Rearrange the data
def RearangeData(ecgdata):
    rawdata = ecgdata["DecodedEcgChannel1"]

    rawdata = [x for x in rawdata if x is not None]
    rawdatalist = list()
    for batch in rawdata:
        if len(batch) < 12:
            continue
        for sample in batch:
            rawdatalist.append(sample)

    rawdata = np.array(rawdatalist)

    ch1 = rawdata

    rawdata = ecgdata["DecodedEcgChannel2"]

    rawdata = [x for x in rawdata if x is not None]
    rawdatalist = list()
    for batch in rawdata:
        if len(batch) < 12:
            continue
        for sample in batch:
            rawdatalist.append(sample)

    rawdata = np.array(rawdatalist)

    ch2 = rawdata

    rawdata = ecgdata["DecodedEcgChannel3"]

    rawdata = [x for x in rawdata if x is not None]
    rawdatalist = list()
    for batch in rawdata:
        if len(batch) < 12:
            continue
        for sample in batch:
            rawdatalist.append(sample)

    rawdata = np.array(rawdatalist)

    ch3 = rawdata

    return ch1, ch2, ch3


def CWT_filter(ch1, ch2, ch3):

    channels = [ch1, ch2, ch3]
    filtered_data_dict = dict()

    # Define the wavelet
    wavelet = "bior3.1"
    for i, channel in enumerate(channels):
        # Perform the wavelet transform
        coeffs = pywt.swt(channel, wavelet, level=1)

        # Apply wavelet thresholding to the coefficients
        threshold = np.std(channel)

        # Apply the filter to the coefficients
        filtered_coeffs = []
        for j in range(len(coeffs)):
            cA, cD = coeffs[j]
            cD_thresholded = pywt.threshold(cD, threshold, mode="soft")
            filtered_coeffs.append((cA, cD_thresholded))

        # Reconstruct the filtered signal
        filtered_data = pywt.iswt(filtered_coeffs, wavelet)

        # To reduce drift in signal, a bandpass filter is applied
        order = 4
        low_freq = 0.5
        high_freq = 40

        # Create the high pass Butterworth filter
        b, a = signal.butter(order, [low_freq, high_freq], fs=250, btype="band")

        # Apply the filter to the filtered data
        filtered_data = signal.filtfilt(b, a, filtered_data)

        # Add the filtered data to the dictionary with a key named after the iteration number
        filtered_data_dict[f"ch{i+1}_filtered"] = list(map(int, filtered_data.tolist()))

    return filtered_data_dict


# endregion

# region Step 3 + 4: QRSDetector + RR-interval => CSI and ModCSI


# Calculate the parametres from the ecg data
def CalcParametres(data):
    # Will return a dictionary with the parametres calculated from the ecg data
    # This is QRS detection
    qrs_detector = QRSDetector(data, plot_data=False, show_plot=False)

    # Extract the qrs peaks and the peak values
    peakIdx = qrs_detector.qrs_peaks_indices
    peakval = qrs_detector.qrs_peak_value

    # Calculate the RR intervals in ms
    rr_interval = []
    for idx in np.arange(len(peakIdx) - 1):
        rr_interval.append(((peakIdx[idx + 1] - peakIdx[idx]) / 250) * 1000)

    # Create a dictionary with the parametres to return
    param = dict()
    param["len_rr"] = len(rr_interval)

    # There should be at least 5 RR intervals to calculate the parametres
    if len(rr_interval) > 4:

        # find the csi 30, 50 and 100
        if len(rr_interval) > 30:
            res = rr_interval[-30:]
            inter30 = get_csi_cvi_features(res)
            param["CSI30"] = inter30["csi"]
            param["ModCSI30"] = inter30["Modified_csi"]
            inter30 = get_time_domain_features(res)
            param["mean_hr30"] = inter30["mean_hr"]
        else:
            param["CSI30"] = 0
            param["ModCSI30"] = 0
            param["mean_hr30"] = 0

        if len(rr_interval) > 50:
            res = rr_interval[-50:]
            inter50 = get_csi_cvi_features(res)
            param["CSI50"] = inter50["csi"]
            param["ModCSI50"] = inter50["Modified_csi"]
            inter50 = get_time_domain_features(res)
            param["mean_hr50"] = inter50["mean_hr"]
        else:
            param["CSI50"] = 0
            param["ModCSI50"] = 0
            param["mean_hr50"] = 0

        if len(rr_interval) > 100:
            res = rr_interval[-100:]
            inter100 = get_csi_cvi_features(res)
            param["CSI100"] = inter100["csi"]
            param["ModCSI100"] = inter100["Modified_csi"]
            inter100 = get_time_domain_features(res)
            param["mean_hr100"] = inter100["mean_hr"]
        else:
            param["CSI100"] = 0
            param["ModCSI100"] = 0
            param["mean_hr100"] = 0

        # Calculate the time domain parametres
        # HR Mean, , etc.
        time_domain_features_all = get_time_domain_features(rr_interval)
        param["mean_hr"] = time_domain_features_all["mean_hr"]

        # Calculate the non-linear domain parametres
        # CSI, Modified CSI, etc. for the full RR interval signal
        NonLineardomainfeatures = get_csi_cvi_features(rr_interval)
        param["csi"] = NonLineardomainfeatures["csi"]
        param["Modified_csi"] = NonLineardomainfeatures["Modified_csi"]
        param.update(NonLineardomainfeatures)
    else:
        param["CSI30"] = None
        param["CSI50"] = None
        param["CSI100"] = None
        param["ModCSI100"] = None

    return param


# endregion


# region Step 5: Rearrange the data
def RearangeDataBack(ecgObject, Findings_ch1, Filtered_data):
    allParametres = dict()

    allParametres["PatientID"] = ecgObject["PatientID"]
    allParametres["TimeStamp"] = ecgObject["TimeStamp"]
    # Sender kun findings for ch1
    allParametres["CSI30"] = Findings_ch1["CSI30"]
    allParametres["CSI50"] = Findings_ch1["CSI50"]
    allParametres["CSI100"] = Findings_ch1["CSI100"]
    allParametres["ModCSI100"] = Findings_ch1["ModCSI100"]
    # Sender filtreret data for all kanaler
    allParametres["ProcessedEcgChannel1"] = Filtered_data["ch1_filtered"]
    allParametres["ProcessedEcgChannel2"] = Filtered_data["ch2_filtered"]
    allParametres["ProcessedEcgChannel3"] = Filtered_data["ch3_filtered"]
    return allParametres


# endregion


# region Step 6: Publish the data
def EncodeJson(dict):

    json_object = json.dumps(dict, allow_nan=True, default=str).encode("utf-8")
    return json_object


def PublishData(json_object):
    try:
        client.publish(Topic_Processed_Measurement, json_object, qos=1)
    except:  # If the data is not in the correct format
        client.publish(
            Topic_Processed_Measurement, "Error".encode("utf-8"), qos=1, retain=True
        )


# endregion


# region Call the functions
def ProcessingAlgorihtm(message):
    try:
        ecgObject = DeserializeJson(message)
        ch1, ch2, ch3 = RearangeData(ecgObject)
        data_filt = CWT_filter(ch1, ch2, ch3)
        Findings_ch1 = CalcParametres(data_filt["ch1_filtered"])

        allParametres = RearangeDataBack(ecgObject, Findings_ch1, data_filt)
        json_object = EncodeJson(allParametres)
        PublishData(json_object)
    except Exception as e:
        errorMsg = "An error occured in the processing algorithm." + "\n" + str(e)
        encoded = errorMsg.encode("utf-8")
        client.publish(Topic_Processed_Measurement, encoded, qos=1)
        print("An error occured in the processing algorithm. \n")
        print(e)


# endregion

"""Running code"""
client = paho.Client(
    callback_api_version=paho.CallbackAPIVersion.VERSION1,
    client_id="",
    userdata=None,
    protocol=paho.MQTTv5,
)
client.on_connect = on_connect
client.tls_set(tls_version=mqtt.client.ssl.PROTOCOL_TLS)
client.on_message = on_message
client.on_disconnect = on_disconnect
client.on_subscribe = on_subscribe
client.on_publish = on_publish
client.on_publish_error = on_publish_error

broker_address = "telemonmqtt-wxmbnq.a01.euc1.aws.hivemq.cloud"
username = "TelemonBroker"
password = "RememberTheStack123"
print("Connecting to MQTT broker... \n with host: " + broker_address)
try:
    client.username_pw_set(username, password)
    client.connect(
        host=broker_address,
        port=8883,
    )

except Exception as e:
    msg = ("Connecting to the broker failed. \n" + e).encode("utf-8")
    print(msg)

# Will run the client forever, unless you interrupt it
client.loop_forever()
