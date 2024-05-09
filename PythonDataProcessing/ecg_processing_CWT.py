# region Import libraries
import paho.mqtt.client as mqtt  # pip install paho-mqtt
from QRSDetector import QRSDetector  # pip install QRSDetector
from hrvanalysis import *  # pip install hrvanalysis
import scipy.signal as signal  # pip install scipy
import numpy as np  # pip install numpy
import json  # pip install json
import datetime  # pip install datetime
import pywt  # pip install pywt

import os


# region Step 0: Setup the MQTT client

# region Topics
Topic_Decoded_Measurement = "ecg_data_group1/decoded_measurements"
Topic_Processed_Measurement = "ecg_data_group1/processed_measurements"
# endregion


# will subscribe to the following topics on the broker
# and publish the last will message on disconnect
def on_connect(client, userdata, flags, rc):
    client.subscribe(Topic_Decoded_Measurement)
    client.subscribe(Topic_Processed_Measurement)

    print("Connected with result code " + str(rc))


# When a message is received, this function is called
def on_message(client, userdata, msg):

    # Decode the message payload and check the topic
    messageEncoded = msg.payload
    message = messageEncoded.decode("utf-8")

    if msg.topic == Topic_Decoded_Measurement:  # Handle the raw data
        ProcessingAlgorihtm(message)
    else:
        print("Unknown topic: " + msg.topic + "\n\t" + str(message))

    # print("=========  END on_message ============================")


# When the client disconnects from the broker
def on_disconnect(client, userdata, rc):
    # client.publish(Topic_Status_Python+"/disconnect", "Disconnected", qos=1, retain=True)
    print("Disconnected with result code " + str(rc))


# When the client subscribes to a topic
def on_subscribe(client, userdata, mid, granted_qos):
    print("Subscribed: " + str(mid) + " QOS:" + str(granted_qos))


# endregion

# region Step 1: Get the data
""" To get the data this project will use MQTT protocol
    The data will be provided from the ASP.Net server on the topic "ECG/Series/Raw"
    It will be a json string thats need to be decoded
"""


# Deserialize the json string
def DeserializeJson(ms):
    # Ready for the real data type, but it requires some changes to match the new dataformat
    ecgObject = json.loads(ms)

    # Read in a ECG from af csv file
    # ecgObject = np.genfromtxt(ms, delimiter=",")
    # ecgObject = ecgObject[1:500, 1]

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
    filtered_data_list = list()

    # Define the wavelet
    wavelet = "bior3.1"
    for i, channel in enumerate(channels):
        # Perform the wavelet transform
        coeffs = pywt.swt(channel, wavelet, level=3)

        # Apply wavelet thresholding to the coefficients
        threshold = np.std(channel)
        # RV: Kig ind i metoder til udregning af thres og mode for thresholding

        # Apply the filter to the coefficients
        filtered_coeffs = []
        for j in range(len(coeffs)):
            cA, cD = coeffs[j]
            cD_thresholded = pywt.threshold(cD, threshold, mode="soft")
            filtered_coeffs.append((cA, cD_thresholded))

        # Reconstruct the filtered signal
        filtered_data = pywt.iswt(filtered_coeffs, wavelet)

        # Define the bandpass filter parameters
        order = 4
        low_freq = 0.5
        high_freq = 40

        # Create the high pass Butterworth filter
        b, a = signal.butter(order, [low_freq, high_freq], fs=250, btype="band")

        # Apply the filter to the filtered data
        filtered_data = signal.filtfilt(b, a, filtered_data)

        # Add the filtered data to the dictionary with a key named after the iteration number
        filtered_data_dict[f"ch{i+1}_filtered"] = filtered_data.tolist()

    return filtered_data_dict


# endregion


# region Step 3 + 4: QRSDetector + RR-interval => CSI and ModCSI


def TimeDiffer(ecgObject):
    # Calculate the time it took to get and process the data
    t1 = ecgObject["TimeStamp"][-1] / 1000
    t2 = datetime.datetime.now().timestamp()
    tdif = t2 - t1
    return tdif


##
# RV noter: Thresholding på CSI skal fjernes. Dict "param" skal indeholde udregnet csi og modcsi værdier samt filteret data, men ikke findings for hver kanal
##
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

    # Add the RR intervals and the filtered ecg to the dictionary
    # param['rr_intervals_ms'] = rr_interval
    # param['filtered_ecg'] = qrs_detector.filtered_ecg_measurements.tolist()

    return param


# endregion


# region Step 5: Decission support
def DecissionSupport(CSINormMax, ModCSINormMax, ch):
    """Due to the reshearsh of Jesper Jeppesen

    CSI30 will be 1.65 times over the normal value
    CSI50 will be 2.15 times over the normal value
    CSI100 will be 1.57 times over the normal value
    ModCSI30 is not used
    ModCSI50 is not used
    ModCSI100 will be 1.80 times over the normal value
    """
    alarm = dict()
    multipleRR = ch["len_rr"] > 4
    if multipleRR and (ch["CSI30"] / CSINormMax[0]) > 1.05:  # 1.65:
        alarm["CSI30_Alarm"] = True  # "Seizure"
    else:
        alarm["CSI30_Alarm"] = False  # "No seizure"

    if multipleRR and (ch["CSI50"] / CSINormMax[1]) > 1.05:  # 2.15:
        alarm["CSI50_Alarm"] = True  # "Seizure"
    else:
        alarm["CSI50_Alarm"] = False  # "No seizure"

    if multipleRR and (ch["CSI100"] / CSINormMax[2]) > 1.05:  # 1.57:
        alarm["CSI100_Alarm"] = True  # "Seizure"
    else:
        alarm["CSI100_Alarm"] = False  # "No seizure"

    if multipleRR and (ch["ModCSI100"] / ModCSINormMax[2]) > 1.05:  # 1.80:
        alarm["ModCSI100_Alarm"] = True  # "Seizure"
    else:
        alarm["ModCSI100_Alarm"] = False  # "No seizure"

    return alarm


# endregion

# region Step 6: Rearrange the data


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


# region Step 7: Publish the data
def EncodeJson(dict):

    json_object = json.dumps(dict, indent=3, allow_nan=True, default=str).encode(
        "utf-8"
    )
    return json_object


def PublishData(json_object):
    try:
        client.publish(Topic_Processed_Measurement, json_object)
    except:  # If the data is not in the correct format
        client.publish(
            Topic_Processed_Measurement, "Error".encode("utf-8"), qos=1, retain=True
        )


# endregion


# region Call the functions
def ProcessingAlgorihtm(message):
    try:
        # Save the JSON message locally
        # with open("message.json", "w") as file:
        #   file.write(message)
        # ecgObject = DeserializeJson(message)
        ecgObject = message
        ch1, ch2, ch3 = RearangeData(ecgObject)
        data_filt = CWT_filter(ch1, ch2, ch3)
        Findings_ch1 = CalcParametres(data_filt["ch1_filtered"])

        allParametres = RearangeDataBack(ecgObject, Findings_ch1, data_filt)
        json_object = EncodeJson(allParametres)
        PublishData(json_object)
    except Exception as e:
        errorMsg = "An error occured in the processing algorithm." + "\n" + str(e)
        encoded = errorMsg.encode("utf-8")
        # client.publish(Topic_Processed_Measurement, encoded, qos=1, retain=True)
        print("An error occured in the processing algorithm. \n")
        print(e)


def load_json_file(file_path):
    with open(file_path, "r") as file:
        data = json.load(file)
    return data


# endregion

"""Running code"""
# region Set up and run the MQTT client
# When the client connects to the broker

client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION1, protocol=mqtt.MQTTv311)
client.on_connect = on_connect
# client.on_message = on_message

client.on_disconnect = on_disconnect
client.on_subscribe = on_subscribe


# Create the last will message
# client.will_set(Topic_Decoded_Measurement, payload=None, qos=0, retain=False)
# client.username_pw_set(username="s1", password="passwordfors1")
broker_address = "test.mosquitto.org"
print("Connecting to MQTT broker... \n with host: " + broker_address)
try:
    # client.username_pw_set(username="s1",password="passwordfors1")
    client.connect(host=broker_address, port=1883, keepalive=120)

except ConnectionRefusedError as e:
    msg = (
        "First attempt to connect fails because the docker container for the MQTT server needs to start."
        + "\n"
        + e
    ).encode("utf-8")
    client.publish(Topic_Processed_Measurement, msg, qos=1, retain=True)

    print(
        "First attempt to connect fails because the docker container for the MQTT server needs to start."
    )
    print(e)
except Exception as e:
    msg = (
        "First attempt to connect fails because the docker container for the MQTT server needs to start."
        + "\n"
        + e
    ).encode("utf-8")
    # client.publish(Topic_Processed_Measurement, msg, qos=1, retain=True)
    print("An error occured in the processing algorithm. \n")
    print(str(e))
data = load_json_file("message.json")
ProcessingAlgorihtm(data)
# Will run the client forever, unless you interrupt it
client.loop_forever()

# endregion
