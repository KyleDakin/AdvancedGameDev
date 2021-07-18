/*
 * Kyle Dakin
 * 1659221
 * NHE2422 Advanced Game Development
 * Innovative UI Development of a VR/AR Game
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

//Adapted from https://www.raywenderlich.com/5475-introduction-to-using-opencv-with-unity

namespace Managers
{
    public class PlayerController : MonoBehaviour
    {
        public GameObject player;
        public Camera camera;
        
        private Thread _receivePlayerThread;
        private Thread _receiveCameraThread;
        
        private UdpClient _playerClient;
        private UdpClient _cameraClient;
        
        private int _playerPort;
        private int _cameraPort;

        //Reference to the CameraContol and PlayerMovement script to communicate the input from the Python programmes
        private PlayerMovement _playerMovement; 
        private CameraControl _cameraControl;
    
        // Start is called before the first frame update
        void Start()
        {
            _playerPort = 52118;
            _cameraPort = 52121;

            //Caches the PlayerMovement and CameraControls so they don't have to found each frame
            _playerMovement = player.GetComponent<PlayerMovement>();
            _cameraControl = camera.GetComponent<CameraControl>();
        
            InitUDP();
        }

        //Creates a thread for the player controls and the camera controls.
        //Each gets their own thread for performance reasons
        void InitUDP()
        {
            _receivePlayerThread = new Thread(new ThreadStart(ReceivePlayerData));
            _receivePlayerThread.IsBackground = true;
            _receivePlayerThread.Start();
            
            _receiveCameraThread = new Thread(new ThreadStart(ReceiveCameraData));
            _receiveCameraThread.IsBackground = true;
            _receiveCameraThread.Start();
        }

        private void ReceivePlayerData()
        {
            _playerClient = new UdpClient (_playerPort);
            while (true)
            {
                try
                {
                    IPEndPoint playerIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _playerPort);
                
                    byte[] playerData = _playerClient.Receive(ref playerIP);

                    string playerText = Encoding.UTF8.GetString(playerData);
                    string[] playerSplit = playerText.Split(char.Parse(" "));

                    _playerMovement.ParseInput(playerSplit[1], playerSplit[2]);

                } 
                catch(Exception e)
                {
                    print (e.ToString());
                }
            }
        }
        
        private void ReceiveCameraData()
        {
            _cameraClient = new UdpClient(_cameraPort);
            while (true)
            {
                try
                {
                    IPEndPoint cameraIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _cameraPort);
                
                    byte[] cameraData = _cameraClient.Receive(ref cameraIP);

                    string cameraText = Encoding.UTF8.GetString(cameraData);
                    string[] cameraSplit = cameraText.Split(char.Parse(" "));
                    
                    _cameraControl.ParseInput(cameraSplit[1], cameraSplit[2], cameraSplit[3]);

                } 
                catch(Exception e)
                {
                    print (e.ToString());
                }
            }
        }

    }
}
