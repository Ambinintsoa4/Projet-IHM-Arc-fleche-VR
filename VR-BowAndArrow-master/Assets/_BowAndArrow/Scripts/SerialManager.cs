using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
public class SerialManager : MonoBehaviour
{
    SerialPort stream;
    bool SerialComm = false;//Used to comm with Arduino or not

    // Start is called before the first frame update
    void Start()
    {
        if (SerialComm)
        {
            stream = new SerialPort("COM8", 115200);  //!!!!!!!!!!!!!!!!!! Need to search and find the good COM port to use the Arduino!!!!!!!!!!!!!!!!!!!!!!
            stream.ReadTimeout = 5;
            stream.Open();
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
