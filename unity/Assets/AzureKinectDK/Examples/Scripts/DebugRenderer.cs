using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;

public class DebugRenderer : MonoBehaviour
{
    Device device;
    BodyTracker tracker;
    Skeleton skeleton;
    GameObject[] debugObjects;
    //public Renderer renderer;

    private void OnEnable()
    {
        this.device = Device.Open(0);
        var config = new DeviceConfiguration
        {
            ColorResolution = ColorResolution.r720p,
            ColorFormat = ImageFormat.ColorBGRA32,
            DepthMode = DepthMode.NFOV_Unbinned
        };
        device.StartCameras(config);

        var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);

        var trackerConfiguration = new TrackerConfiguration {
            SensorOrientation = SensorOrientation.OrientationDefault,
            CpuOnlyMode = false
        };
        this.tracker = BodyTracker.Create(calibration, trackerConfiguration);
        debugObjects = new GameObject[(int)JointType.Count];
        for (var i = 0; i < (int)JointType.Count; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = Enum.GetName(typeof(JointType), i);
            cube.transform.localScale = Vector3.one * 0.4f;
            debugObjects[i] = cube;
        }
    }

    private void OnDisable()
    {
        if (tracker != null)
        {
            tracker.Dispose();
        }
        if (device != null)
        {
            device.Dispose();
        }
    }

    void Update()
    {
        using (Capture capture = device.GetCapture())
        {
            tracker.EnqueueCapture(capture);
            // TODO  DO NOT USE IT!!! It causes memory leak! 
            /*var color = capture.Color;
            if (color != null && color.WidthPixels > 0)
            {
                Texture2D tex = new Texture2D(color.WidthPixels, color.HeightPixels, TextureFormat.BGRA32, false);
                tex.LoadRawTextureData(color.GetBufferCopy());
                tex.Apply();
                renderer.material.mainTexture = tex;
            }*/
        }
        
        using (BodyFrame frame = tracker.PopResult())
        {
            Debug.LogFormat("{0} bodies found.", frame.BodyCount);
            if (frame.BodyCount > 0)
            {
                var bodies = new List<Body>();
                frame.GetBodies(addBody, ref bodies);

                var body = bodies[0];
                foreach (var pair in body.Joints) {

                    var joint = pair.Value;
                    var pos = joint.Position;
                    var orientation = joint.Orientation;
                    var v = new Vector3(pos.X, -pos.Y, pos.Z) * 0.004f;
                    var r = new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);
                    var obj = debugObjects[(int)pair.Key];
                    obj.transform.SetPositionAndRotation(v, r);
                }
            }
        }
    }

    private void addBody(Body body, ref List<Body> collection) {
        collection.Add(body);
    }
}
