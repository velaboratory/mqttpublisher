using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine.Android;
using System;
using UnityEngine.UI;

public class MQTTPublisher : MonoBehaviour
{
	MqttClient mqttClient;
	public Transform attitudeVisual;
	public TMP_Text allSensorsText;
	public TMP_Text buttonText;
	public TMP_InputField user;
	public TMP_InputField pw;
	public TMP_InputField broker;
	public TMP_InputField port;
	public TMP_InputField id;
	public TMP_InputField rootTopic;
	public Button startButton;
	public Toggle sslToggle;
	bool hasLocationPermission = false;
	
	// Start is called before the first frame update
	void Start()
	{
		
#if UNITY_ANDROID

		PermissionCallbacks callbacks = new PermissionCallbacks();
		callbacks.PermissionGranted += (s) =>
		{
			hasLocationPermission = true;
		};
		
		if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
		{
			UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation,callbacks);
		}




#endif

		broker.text = PlayerPrefs.GetString("broker", "");
		port.text = PlayerPrefs.GetString("port", "" + 1883);
		user.text = PlayerPrefs.GetString("user", "");
		pw.text = PlayerPrefs.GetString("pw", "");
		id.text = PlayerPrefs.GetString("id", SystemInfo.deviceUniqueIdentifier);
		sslToggle.isOn = PlayerPrefs.GetInt("ssl", 0) > 0;
		rootTopic.text = PlayerPrefs.GetString("root", "mqttpublisher");


	}

	public void OnApplicationFocus(bool focus)
	{
		if (focus) { return; }
		if (mqttClient != null)
		{
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
			Input.location.Stop();
			Application.targetFrameRate = 30;

			if (AttitudeSensor.current != null)
			{
				InputSystem.DisableDevice(AttitudeSensor.current);
			}
			if (Accelerometer.current != null)
			{
				InputSystem.DisableDevice(Accelerometer.current);
			}
			if (Gyroscope.current != null)
			{
				InputSystem.DisableDevice(Gyroscope.current);
			}
			if (LinearAccelerationSensor.current != null)
			{
				InputSystem.DisableDevice(LinearAccelerationSensor.current);
			}
			if (GravitySensor.current != null)
			{
				InputSystem.DisableDevice(GravitySensor.current);
			}

			EnhancedTouchSupport.Disable();

			//android only
			if (LightSensor.current != null)
			{
				InputSystem.DisableDevice(LightSensor.current);
			}

			if (MagneticFieldSensor.current != null)
			{
				InputSystem.DisableDevice(MagneticFieldSensor.current);
			}

			if (PressureSensor.current != null)
			{
				InputSystem.DisableDevice(PressureSensor.current);
			}

			if (ProximitySensor.current != null)
			{
				InputSystem.DisableDevice(ProximitySensor.current);
			}

			if (HumiditySensor.current != null)
			{
				InputSystem.DisableDevice(HumiditySensor.current);
			}

			if (AmbientTemperatureSensor.current != null)
			{
				InputSystem.DisableDevice(AmbientTemperatureSensor.current);
			}
			mqttClient.Disconnect();
			mqttClient = null;
			startButton.gameObject.SetActive(true);
			buttonText.text = "Start";
			allSensorsText.text = "Click Start To Connect";
			startButton.interactable = true;
			broker.interactable = true;
			user.interactable = true;
			pw.interactable = true;
			rootTopic.interactable = true;
			id.interactable = true;
			port.interactable = true;
			sslToggle.interactable = true;
			
		}
	}

	public void startClicked()
	{
		
		
		try
		{
			if (sslToggle.isOn)
			{
				mqttClient = new MqttClient(broker.text, int.Parse(port.text), true, null, null, MqttSslProtocols.SSLv3);
			}
			else
			{
				mqttClient = new MqttClient(broker.text, int.Parse(port.text), false, null, null, MqttSslProtocols.None);
			}
			mqttClient.Connect(id.text, user.text, pw.text);

			if (mqttClient.IsConnected)
			{
				PlayerPrefs.SetString("broker", broker.text);
				PlayerPrefs.SetString("user", user.text);
				PlayerPrefs.SetString("pw", pw.text);
				PlayerPrefs.SetString("id", id.text);
				PlayerPrefs.SetString("root", rootTopic.text);
				PlayerPrefs.SetString("port", port.text);
				PlayerPrefs.SetInt("ssl", sslToggle.isOn ? 1 : 0);
				buttonText.text = "Stop";
			}
			else
			{
				mqttClient = null;
				allSensorsText.text = "Invalid username or password";
			}
		}
		catch (Exception e)
		{
			mqttClient = null;
			Debug.LogException(e);
			allSensorsText.text = "Invalid broker";
			return;
		}

		startButton.interactable = false;
		broker.interactable = false;
		user.interactable = false;
		port.interactable = false;
		sslToggle.interactable = false;
		pw.interactable = false;
		rootTopic.interactable = false;
		id.interactable = false;
		startButton.gameObject.SetActive(false);




		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		if (hasLocationPermission)
		{
			Input.location.Start(1, 0);
		}
		Application.targetFrameRate = 1000;

		if (AttitudeSensor.current != null)
		{
			InputSystem.EnableDevice(AttitudeSensor.current);
		}
		if (Accelerometer.current != null)
		{
			InputSystem.EnableDevice(Accelerometer.current);
		}
		if (Gyroscope.current != null)
		{
			InputSystem.EnableDevice(Gyroscope.current);
		}
		if (LinearAccelerationSensor.current != null)
		{
			InputSystem.EnableDevice(LinearAccelerationSensor.current);
		}
		if (GravitySensor.current != null)
		{
			InputSystem.EnableDevice(GravitySensor.current);
		}

		EnhancedTouchSupport.Enable();

		//android only
		if (LightSensor.current != null)
		{
			InputSystem.EnableDevice(LightSensor.current);
		}

		if (MagneticFieldSensor.current != null)
		{
			InputSystem.EnableDevice(MagneticFieldSensor.current);
		}

		if (PressureSensor.current != null)
		{
			InputSystem.EnableDevice(PressureSensor.current);
		}

		if (ProximitySensor.current != null)
		{
			InputSystem.EnableDevice(ProximitySensor.current);
		}

		if (HumiditySensor.current != null)
		{
			InputSystem.EnableDevice(HumiditySensor.current);
		}

		if (AmbientTemperatureSensor.current != null)
		{
			InputSystem.EnableDevice(AmbientTemperatureSensor.current);
		}


		

	}

	
	
	// Update is called once per frame
	void Update()
	{
		if (mqttClient != null && mqttClient.IsConnected)
		{
			allSensorsText.text = "";

			if (AttitudeSensor.current != null)
			{
				publishAttitude();
			}
			if (Accelerometer.current != null)
			{
				publishAccel();
			}
			if (Gyroscope.current != null)
			{
				publishGyro();
			}
			if (LinearAccelerationSensor.current != null)
			{
				publishLinearAcceleration();
			}
			if (GravitySensor.current != null)
			{
				publishGravity();
			}


			//android only
			if (LightSensor.current != null)
			{
				publishLight();
			}

			if (MagneticFieldSensor.current != null)
			{
				publishMagnetic();
			}

			if (PressureSensor.current != null)
			{
				publishPressure();
			}

			if (ProximitySensor.current != null)
			{
				publishProximity();
			}

			if (HumiditySensor.current != null)
			{
				publishHumidity();
			}

			if (AmbientTemperatureSensor.current != null)
			{
				publishTemperature();
			}
			if (Input.location.isEnabledByUser)
			{
				publishLocation();
			}
			publishTouches();
		}



	}

	void publishAttitude()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		Quaternion q = AttitudeSensor.current.attitude.ReadValue();
		Quaternion r = Quaternion.Euler(90, 0, 0) * new Quaternion(q.x, q.y, -q.z, -q.w); //switch lh and rh coordinate systems and rotate
		attitudeVisual.rotation = r;
		writer.Write(r.x);
		writer.Write(r.y);
		writer.Write(r.z);
		writer.Write(r.w);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/attitude", ms.ToArray());
		
		allSensorsText.text = "Att: "+ r.ToString() + "\n";
	}

	void publishAccel()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		Vector3 accel = Accelerometer.current.acceleration.ReadValue();
		writer.Write(accel.x);
		writer.Write(accel.y);
		writer.Write(accel.z);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/raw_accel", ms.ToArray());
		allSensorsText.text += "Accel: " + accel.ToString() + "\n";
	}

	void publishTouches()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		writer.Write(Touch.activeFingers.Count);
		allSensorsText.text += "Touches: ";
		foreach (var finger in Touch.activeFingers)
		{
			Vector2 p = finger.currentTouch.screenPosition;
			writer.Write(finger.index);
			writer.Write(p.x);
			writer.Write(p.y);
			writer.Write(finger.currentTouch.isTap);
			writer.Write(finger.currentTouch.ended);
			writer.Write(finger.currentTouch.began);
			allSensorsText.text += "\n  " +finger.index + ":" + p.x+"\t"+p.y;
		}

		mqttClient.Publish(rootTopic.text + "/" + id.text + "/touches", ms.ToArray());
		allSensorsText.text += "\n";
	}

	void publishGyro()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		Vector3 gyro = Gyroscope.current.angularVelocity.ReadValue();
		writer.Write(gyro.x);
		writer.Write(gyro.y);
		writer.Write(gyro.z);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/gryo", ms.ToArray());
		allSensorsText.text += "Gyro: " + gyro + "\n";
	}
	void publishLinearAcceleration()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		Vector3 accel = LinearAccelerationSensor.current.acceleration.ReadValue();
		writer.Write(accel.x);
		writer.Write(accel.y);
		writer.Write(accel.z);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/linear_accel", ms.ToArray());
		allSensorsText.text += "Linear: " + accel + "\n";
	}

	void publishGravity()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		Vector3 gravity = GravitySensor.current.gravity.ReadValue();
		writer.Write(gravity.x);
		writer.Write(gravity.y);
		writer.Write(gravity.z);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/gravity", ms.ToArray());
		allSensorsText.text += "Grav: " + gravity + "\n";
	}

	void publishLight() {
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		float light = LightSensor.current.lightLevel.ReadValue();
		writer.Write(light);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/light", ms.ToArray());
		allSensorsText.text += "Light: " + light + "\n";
	}
	void publishHumidity() {
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		float humidity = HumiditySensor.current.relativeHumidity.ReadValue();
		writer.Write(humidity);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/humidity", ms.ToArray());
		allSensorsText.text += "Humidity: " + humidity + "\n";
	}
	void publishProximity() {
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		float distance = ProximitySensor.current.distance.ReadValue();
		writer.Write(distance);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/proximity", ms.ToArray());
		allSensorsText.text += "Proximity: " + distance + "\n";
	}
	void publishTemperature()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		float ambientTemperature = AmbientTemperatureSensor.current.ambientTemperature.ReadValue();
		writer.Write(ambientTemperature);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/temperature", ms.ToArray());
		allSensorsText.text += "Temperature: " + ambientTemperature + "\n";
	}
	void publishMagnetic()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		Vector3 magneticField = MagneticFieldSensor.current.magneticField.ReadValue();
		writer.Write(magneticField.x);
		writer.Write(magneticField.y);
		writer.Write(magneticField.z);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/magnetic", ms.ToArray());
		allSensorsText.text += "Mag: " + magneticField + "\n";
	}
	void publishPressure()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		float atmosphericPressure = PressureSensor.current.atmosphericPressure.ReadValue();
		writer.Write(atmosphericPressure);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/pressure", ms.ToArray());
		allSensorsText.text += "Pressure: " + atmosphericPressure + "\n";
	}

	void publishLocation()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);
		float latitude = Input.location.lastData.latitude;
		float longitude = Input.location.lastData.longitude;
		float altitude = Input.location.lastData.altitude;
		float horizontalAccuracy = Input.location.lastData.horizontalAccuracy;
		float verticalAccuracy = Input.location.lastData.verticalAccuracy ;
		double time = Input.location.lastData.timestamp;
		
		writer.Write(latitude);
		writer.Write(longitude);
		writer.Write(altitude);
		writer.Write(horizontalAccuracy);
		writer.Write(verticalAccuracy);
		writer.Write(time);
		mqttClient.Publish(rootTopic.text + "/" + id.text + "/location", ms.ToArray());
		allSensorsText.text += "Lat: " + latitude + "\n";
		allSensorsText.text += "Long: " + longitude + "\n";
		allSensorsText.text += "Alt: " + altitude + "\n";
		allSensorsText.text += "Accuracy: " + horizontalAccuracy + "," + verticalAccuracy+"\n";
	}

}
