using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

using Facebook.Unity;

namespace com.MJT.FindTheThief
{
    public class Launcher : Photon.PunBehaviour
    {
        private readonly string _gameVersion = "0.1";

        // Use this for initialization
        void Awake()
        {
            // PhotonNetwork.LoadLevel()가 마스터 클라이언트에서 호출되면 타 플레이어들도 자동으로 싱크됨
            PhotonNetwork.automaticallySyncScene = true;
            // 마스터 서버에 접속했을 때 자동으로 로비에 접속한다.
            PhotonNetwork.autoJoinLobby = false;

            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback);
            }/*
        else
        {
            FacebookLogin();
        }*/
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        //페이스북 로그인 버튼을 누르면 호출되는 로그인 시도 함수
        public void FacebookLogin()
        {
            //로그인된 상태라면 바로 로그인시 호출함수(OnFacebookLoggedIn)로 넘어가고, 아니면 설정된 권한으로 로그인 시도
            if (FB.IsLoggedIn)
            {
                OnFacebookLoggedIn();
            }
            else
            {
                var perms = new List<string>() { "public_profile", "email", "user_friends" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
        }

        //페이스북 초기화(FB.Init)시 호출되는 콜백
        private void InitCallback()
        {
            //초기화되면 페이스북 로그인, 초기화에 실패하면 오류 메시지 출력
            if (!FB.IsInitialized)
            {
                Debug.Log("Failed to initialize the Facebook SDK");
            }
        }

        //LogInWithReadPermissions 시도시 호출되는 콜백
        private void AuthCallback(ILoginResult result)
        {
            //로그인이 정상적으로 수행되면 OnFacebookLoggedIn로 이동, 아니면 에러
            if (FB.IsLoggedIn)
            {
                OnFacebookLoggedIn();
            }
            else
            {
                //Debug.Log(result.Error);
                Debug.LogErrorFormat("Error in Facebook login {0}", result.Error);
            }
        }

        //Facebook의 토큰과 유저 아이디로 Photon 접속 시도
        private void OnFacebookLoggedIn()
        {
            // AccessToken class will have session details
            string aToken = AccessToken.CurrentAccessToken.TokenString;
            string facebookId = AccessToken.CurrentAccessToken.UserId;
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Facebook;
            PhotonNetwork.AuthValues.UserId = facebookId; // alternatively set by server
            PhotonNetwork.AuthValues.AddAuthParameter("token", aToken);
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }

        public void TestLogin()
        {
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }

        // if AutoJoinLobby is false
        public override void OnConnectedToMaster()
        {
            Debug.Log("Successfully connected to Photon!");
            SceneManager.LoadScene("Lobby");
        }

        // if AutoJoinLobby is true
        public override void OnJoinedLobby()
        {
            Debug.Log("Successfully connected to Photon and joined to a lobby!");
            SceneManager.LoadScene("Lobby");
        }

        // something went wrong
        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
            Debug.LogErrorFormat("Error authenticating to Photon using facebook: {0}", debugMessage);
        }
    }
}
