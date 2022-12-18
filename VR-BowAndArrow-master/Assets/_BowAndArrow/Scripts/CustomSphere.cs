using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using UnityEngine;


public class CustomSphere : MonoBehaviour
{

    /* The serial port where the Arduino is connected. */
    [Tooltip("The serial port where the Arduino is connected")]
    public string port = "COM16";
    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 115200;

    private SerialPort stream;


    static UInt16 nbrFrame = 0;
    // Start is called before the first frame update
    void Start()
    {
        Open();
        ResetPosition();
        StartCoroutine(
                //ReadFromFileAsync(
                AsynchronousReadFromArduino(
            (string s) => Move(s), () => Debug.LogError("Error!"), 1000f));
    }

    private bool tracking = false;
    private bool pulling = false;   
    private bool release = false;
    //private const float DefaultPosition = -0.165f;
    private float maxRecordedValue = default(float);
    private float currentValue = default(float);
    private const float PullThreshold = -0.10f;
    private const float ReleaseThreshold = -0.20f;
    private const float defaultZPosition = -0.17f;
    public Transform m_FixedSphere = null;

    #region triggers
    public bool StartPulling()
    {  
        return pulling && !tracking;
    }

    public bool TriggerRelease()
    {
        return release && tracking;
    }
    #endregion



    public void StartTracking()
    {
        tracking = true;
    }

    public void ResetPosition()
    {
        Debug.Log($"ResetPosition# release={release}");
        tracking = false;
        release = false;
        pulling = false;
        maxRecordedValue = default(float);
        currentValue = default(float);
        transform.position = m_FixedSphere.transform.position;
    }

    private bool IsLeftOpSmallerOrEqual(float a, float b)
    {
        return Math.Abs(a) <= Math.Abs(b);
    }

    public void Move(string message)
    {
        //Log(message);
        const float startTrackingValue = defaultZPosition + PullThreshold;
        if (float.TryParse(message, NumberStyles.Float, NumberFormatInfo.InvariantInfo ,out var value))
        {
            var pullValue = RangeScaleValue(value);
            Debug.Log($"Move# Value={value},  Scaled value={pullValue}, startTrackingValue={startTrackingValue}, tracking={tracking}, pulling={pulling}, release={release}, maxRecordedValue={maxRecordedValue}");
            if (!tracking && IsLeftOpSmallerOrEqual(startTrackingValue, pullValue))
            {
                pulling = true;
            }

            if (!pulling)
                return;


            currentValue = pullValue;
            transform.position = m_FixedSphere.transform.position;
            transform.Translate(new Vector3(0, 0, pullValue), m_FixedSphere.transform);
            if (IsLeftOpSmallerOrEqual(maxRecordedValue, currentValue))
            {
                maxRecordedValue = currentValue;
            }

            if (IsLeftOpSmallerOrEqual(currentValue, maxRecordedValue - ReleaseThreshold))
            {
                release = true;
                Debug.Log($"release# release={release}");
            }
        }
    }

    private void Log(string message)
    {
        File.AppendAllLines(@"C:\Users\Ambin\dataESP.txt", Enumerable.Repeat(message, 1));
    }

    public float ScaleValue(float value)
    {
        const float scale = -1 * 3000f; //-1 * 3000f
        const float adjustment = 0f;

        var scaledValue = (value - adjustment) / scale;
        return Math.Min(scaledValue, defaultZPosition);
    }

    public float RangeScaleValue(float value)
    {
        const float maxScale = -0.645f;

        // valeur minimum apres pour laquelle le scale donnera le maxScale
        const float maxValue = 16.95f;
        //const float maxValue = 100;

        // valeur capturee au repos de la corde
        //const float minValue = 980;
        const float minValue = 4.00f;

        if (minValue > value)
            return defaultZPosition;

        if (maxValue < value)
            return maxScale;

        return maxScale * (value - minValue) / maxValue + defaultZPosition;
        //return -1 * (maxScale * (value - minValue) / maxValue + defaultZPosition);
    }

    // Update is called once per frame
    void Update()
    {
        //if (nbrFrame == 0)
        //{
            WriteToArduino("PING");
            //nbrFrame = 60;
        //}
        //else nbrFrame--;

    }

    public void Open()
    {
        // Opens the serial port
        stream = new SerialPort(port, baudrate);
        stream.ReadTimeout = 5; //Go from 50 to 5 to reduce performance issues
        stream.Open();
        //this.stream.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
    }

    public void WriteToArduino(string message)
    {
        // Send the request
        stream.WriteLine(message);
        stream.BaseStream.Flush();
    }

    public IEnumerator ReadFromFileAsync(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        var content = File.ReadAllLines(@"C:\Users\Ambin\data.txt");
        var iterator = 0;
        do
        {
            var value = content[iterator];
            callback(value);

            iterator++;
            yield return new WaitForSeconds(0.05f);
        }
        while (content.Length > iterator);

    }

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            // A single read attempt
            try
            {
                dataString = stream.ReadLine();
                //Debug.Log($"AsynchronousReadFromArduino# data={dataString}");
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null;
            }
            else

                //dataString = stream.ReadLine();
                //callback(dataString);
                //Log(dataString);
                yield return new WaitForSeconds(0.05f);

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }

    public void Close()
    {
        stream.Close();
    }

}
