using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class CameraWork : MonoBehaviour
    {
        #region Public Properties

        public Vector3 centerOffset = Vector3.zero; //Offset of the main camera's location from a player

        public bool followOnStart = false;          //In test, set true. In published game, set false
        public float cameraHeight;                  //The height of the camera. 

        #endregion

        #region Private Properties

        Transform cameraTransform;  //Transform of the main camera.
        bool isFollowing;           //Maintain a flag internally to reconnect if the target is lost or the camera is switched


        #endregion



        #region Unity Callbacks

        // Use this for initialization
        void Start()
        {
            // Start following the target if wanted.
            if (followOnStart)
            {
                OnStartFollowing();
            }
        }

        //A follow camera should always be implemented in LateUpdate.
        //because it tracks objects that might have moved inside Update.
        void LateUpdate()
        {
            //If this scripts is implemeted for multiple scenes and the transform target may not destroy on level load,
            //we need to cover corner cases where the Main Camera is different every time we load a new scene and reconnect when that happens
            if (cameraTransform == null && isFollowing)
            {
                OnStartFollowing();
            }

            // only follow is explicitly declared
            if (isFollowing)
            {
                Follow();
            }
        }

        #endregion


        #region Public Methods

        public void OnStartFollowing()
        {
            cameraTransform = Camera.main.transform;
            isFollowing = true;
            // we don't smooth anything, we go straight to the right camera shot
            Follow();
        }

        #endregion


        #region Private Methods

        private void Follow()
        {
            Vector3 oldCameraPos = cameraTransform.position;
            Vector3 targetCenter = transform.position + centerOffset;
            // Set the position of the camera
            cameraTransform.position = new Vector3(targetCenter.x, targetCenter.y, -cameraHeight);

            UIManager.Instance.MovePopUpsOnCameraMoving(cameraTransform.position - oldCameraPos);
        }

        #endregion
    }
}
