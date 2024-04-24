# region Import libraries
import paho.mqtt.client as mqtt  # pip install paho-mqtt
from QRSDetector import QRSDetector  # pip install QRSDetector
from hrvanalysis import *  # pip install hrvanalysis

import numpy as np  # pip install numpy
import json  # pip install json
import datetime  # pip install datetime

import os


# region Step 0: Setup the MQTT client

# region Topics

pre = "Dev/"
pre = ""

Topic_GetVersion = pre + "ECG/GetVersion"

Topic_Status = pre + "ECG/Status/#"
Topic_Status_CSSURE = pre + "ECG/Status/CSSURE"
Topic_Version_CSSURE = pre + "ECG/Versíon/CSSURE"

Topic_Status_Python = pre + "ECG/Status/Python"
Topic_Version_Python = pre + "ECG/Version/Python"

Topic_Series_Raw = pre + "ECG/Series/CSSURE2PYTHON"
Topic_Series_Filtred = pre + "ECG/Series/PYTHON2CSSURE"

# endregion


# will subscribe to the following topics on the broker
# and publish the last will message on disconnect
def on_connect(client, userdata, flags, rc):
    client.subscribe(Topic_Status)
    client.subscribe(Topic_Series_Raw)
    client.subscribe(Topic_GetVersion)
    client.publish(Topic_Status_Python, "Online".encode("utf-8"), qos=0, retain=True)
    client.publish(Topic_Version_Python, qos=0, retain=True)

    print("Connected with result code " + str(rc))


# When a message is received, this function is called
def on_message(client, userdata, msg):

    # Decode the message payload and check the topic
    messageEncoded = msg.payload
    message = messageEncoded.decode("utf-8")

    # Switch case for the different topics
    if msg.topic == Topic_Status_CSSURE:  # ASP.Net status
        print("ASP.Net status: " + str(message))
        """
        Add some logic here to handle if the ASP.Net status gets offline
        """

    elif msg.topic == Topic_Series_Raw:  # Handle the raw data
        ProcessingAlgorihtm(message)
    elif msg.topic == Topic_GetVersion:  # Handle if somebody req a version
        client.publish(Topic_Version_Python, qos=0, retain=True)
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

    return ecgObject


# endregion


# region Step 2: Rearrange the data
def RearangeData(ecgdata):
    rawdata = ecgdata["ECGChannel1"]

    rawdata = [x for x in rawdata if x is not None]
    rawdatalist = list()
    for batch in rawdata:
        if len(batch) < 12:
            continue
        for sample in batch:
            rawdatalist.append(sample)

    rawdata = np.array(rawdatalist)

    timestamp_mock = np.zeros(len(rawdata))
    rawdata = np.c_[timestamp_mock, rawdata]

    ch1 = rawdata

    rawdata = ecgdata["ECGChannel2"]

    rawdata = [x for x in rawdata if x is not None]
    rawdatalist = list()
    for batch in rawdata:
        if len(batch) < 12:
            continue
        for sample in batch:
            rawdatalist.append(sample)

    rawdata = np.array(rawdatalist)

    timestamp_mock = np.zeros(len(rawdata))
    rawdata = np.c_[timestamp_mock, rawdata]

    ch2 = rawdata

    rawdata = ecgdata["ECGChannel3"]

    rawdata = [x for x in rawdata if x is not None]
    rawdatalist = list()
    for batch in rawdata:
        if len(batch) < 12:
            continue
        for sample in batch:
            rawdatalist.append(sample)

    rawdata = np.array(rawdatalist)

    timestamp_mock = np.zeros(len(rawdata))
    rawdata = np.c_[timestamp_mock, rawdata]

    ch3 = rawdata

    return ch1, ch2, ch3


# endregion

# region Step 3 + 4: QRSDetector + RR-interval => CSI and ModCSI


def TimeDiffer(ecgObject):
    # Calculate the time it took to get and process the data
    t1 = ecgObject["TimeStamp"][-1] / 1000
    t2 = datetime.datetime.now().timestamp()
    tdif = t2 - t1
    return tdif


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


def RearangeDataBack(
    ecgObject, Findings_ch1, Findings_ch2, Findings_ch3, timeDifferent, Alarm
):
    allParametres = dict()

    allParametres["PatientID"] = ecgObject["PatientID"]
    allParametres["TimeStamp"] = ecgObject["TimeStamp"][-1]
    allParametres["TimeProcess_s"] = timeDifferent
    allParametres["SeriesLength_s"] = len(ecgObject["TimeStamp"]) * 12 / 250
    allParametres["Alarm"] = Alarm
    allParametres["ECGChannel1"] = Findings_ch1
    allParametres["ECGChannel2"] = Findings_ch2
    allParametres["ECGChannel3"] = Findings_ch3
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
        client.publish(Topic_Series_Filtred, json_object)
    except:  # If the data is not in the correct format
        client.publish(
            Topic_Series_Filtred, "Error".encode("utf-8"), qos=1, retain=True
        )


# endregion


# region Call the functions
def ProcessingAlgorihtm(message):
    try:
        t1 = datetime.datetime.now().timestamp()
        ecgObject = DeserializeJson(message)
        ch1, ch2, ch3 = RearangeData(ecgObject)
        timeDifferent = TimeDiffer(ecgObject)
        if timeDifferent > 1000:
            client.publish(
                Topic_Series_Filtred, "TimeError".encode("utf-8"), qos=1, retain=True
            )
        else:
            Findings_ch1 = CalcParametres(ch1)
            Findings_ch2 = CalcParametres(ch2)
            Findings_ch3 = CalcParametres(ch3)

            Alarm = DecissionSupport(
                ecgObject["CSINormMax"], ecgObject["ModCSINormMax"], Findings_ch1
            )

            t2 = datetime.datetime.now().timestamp()
            timeDifferent = t2 - t1
            allParametres = RearangeDataBack(
                ecgObject,
                Findings_ch1,
                Findings_ch2,
                Findings_ch3,
                timeDifferent,
                Alarm,
            )
            json_object = EncodeJson(allParametres)
            PublishData(json_object)
    except Exception as e:
        errorMsg = "An error occured in the processing algorithm." + "\n" + str(e)
        encoded = errorMsg.encode("utf-8")
        client.publish(Topic_Series_Filtred, encoded, qos=1, retain=True)
        print("An error occured in the processing algorithm. \n")
        print(e)


# endregion

"""Running code"""
# region Set up and run the MQTT client
# When the client connects to the broker
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message
client.on_disconnect = on_disconnect
client.on_subscribe = on_subscribe


# Create the last will message
client.will_set(Topic_Status_Python, payload="Offline", qos=0, retain=True)
client.username_pw_set(username="s1", password="passwordfors1")
broker_address = "localhost"
broker_address = "assure.au-dev.dk"
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
    client.publish(Topic_Series_Filtred, msg, qos=1, retain=True)

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
    client.publish(Topic_Series_Filtred, msg, qos=1, retain=True)
    print("An error occured in the processing algorithm. \n")
    print(str(e))

# Will run the client forever, unless you interrupt it
client.loop_forever()

# endregion
