using CotcSdk;
using UnityEngine;

#if UNITY_5_4_OR_NEWER
#else
using UnityEngine.Experimental.Networking;
#endif

public class CustomLoginManager : MonoBehaviour
{
    // The cloud allows to make generic operations (non user related)
    private static Cloud Cloud;
    // The gamer is the base to perform most operations. A gamer object is obtained after successfully signing in.
    private static Gamer currentGamer;
    // The friend, when fetched, can be used to send messages and such
    private string FriendId;
    // When a gamer is logged in, the loop is launched for domain private. Only one is run at once.
    private static DomainEventLoop Loop;

    [SerializeField] private CotcGameObject m_CotcGameObject = null;

    private LoginUIManager m_LoginUIManager = null;

    private LeaderboardUIManager m_LeaderboardUIManager = null;

    // Use this for initialization
    void Start()
    {

        if (m_CotcGameObject == null)
        {
            Debug.LogError("Please put a Clan of the Cloud prefab in your scene!");
            return;
        }
        // Log unhandled exceptions (.Done block without .Catch -- not called if there is any .Then)
        Promise.UnhandledException += (object sender, ExceptionEventArgs e) =>
        {
            Debug.LogError("Unhandled exception: " + e.Exception.ToString());
        };
        // Initiate getting the main Cloud object
        m_CotcGameObject.GetCloud().Done(cloud =>
        {
            Cloud = cloud;
            // Retry failed HTTP requests once
            Cloud.HttpRequestFailedHandler = (HttpRequestFailedEventArgs e) =>
            {
                if (e.UserData == null)
                {
                    e.UserData = new object();
                    e.RetryIn(1000);
                }
                else
                    e.Abort();
            };
            Debug.Log("Setup done");
        });
        // Use a default text in the e-mail address
        //EmailInput.text = DefaultEmailAddress;

        // Use a default text in the password
        //PasswordInput.text = DefaultPassword;

        m_LoginUIManager = FindObjectOfType<LoginUIManager>();
        if (m_LoginUIManager != null)
        {
            bool isPlayerLogged = RequireGamer();
            if (isPlayerLogged)
            {
                GetProfileData();
            }
            m_LoginUIManager.ShouldDisplayLogin(isPlayerLogged);
        }

    }

    // Signs in with an anonymous account
    public void DoLogin()
    {
        // Call the API method which returns an Promise<Gamer> (promising a Gamer result).
        // It may fail, in which case the .Then or .Done handlers are not called, so you
        // should provide a .Catch handler.
        Cloud.LoginAnonymously()
            .Then(gamer => DidLogin(gamer))
            .Catch(ex =>
            {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
            });
    }

    // Log in by e-mail
    public void DoLoginEmail(string networdId, string networkSecret)
    {
        // You may also not provide a .Catch handler and use .Done instead of .Then. In that
        // case the Promise.UnhandledException handler will be called instead of the .Done
        // block if the call fails.
        Cloud.Login(
            network: LoginNetwork.Email.Describe(),
            networkId: networdId,
            networkSecret: networkSecret)
        .Done(this.DidLogin);
    }

    public void DoLoginGameCenter()
    {
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                Debug.Log("Authentication successful:\nUsername: " + Social.localUser.userName +
                    "\nUser ID: " + Social.localUser.id +
                    "\nIsUnderage: " + Social.localUser.underage);
                // Game Center accounts do not have a password
                Cloud.Login(LoginNetwork.GameCenter.Describe(), Social.localUser.id, "n/a").Done(this.DidLogin);
            }
            else
            {
                Debug.LogError("Failed to authenticate on Game Center");
            }
        });
    }

    // Converts the account to e-mail
    public void DoConvertToEmail(string networdId, string networkSecret)
    {
        if (!RequireGamer()) return;
        currentGamer.Account.Convert(
            network: LoginNetwork.Email.ToString().ToLower(),
            networkId: networdId,
            networkSecret: networkSecret)
        .Done(dummy =>
        {
            Debug.Log("Successfully converted account");
        });
    }

    // Fetches a friend with the given e-mail address
    public void DoFetchFriend(string address)
    {
        Cloud.ListUsers(filter: address)
        .Done(friends =>
        {
            if (friends.Total != 1)
            {
                Debug.LogWarning("Failed to find account with e-mail " + address + ": " + friends.ToString());
            }
            else
            {
                FriendId = friends[0].UserId;
                Debug.Log(string.Format("Found friend {0} ({1} on {2})", FriendId, friends[0].NetworkId, friends[0].Network));
            }
        });
    }

    // Sends a message to the current friend
    public void DoSendMessage()
    {
        if (!RequireGamer() || !RequireFriend()) return;

        currentGamer.Community.SendEvent(
            gamerId: FriendId,
            eventData: Bundle.CreateObject("hello", "world"),
            notification: new PushNotification().Message("en", "Please open the app"))
        .Done(dummy => Debug.Log("Sent event to gamer " + FriendId));
    }

    // Posts a sample transaction
    public void DoPostTransaction()
    {
        if (!RequireGamer()) return;

        currentGamer.Transactions.Post(Bundle.CreateObject("gold", 50))
        .Done(result =>
        {
            Debug.Log("TX result: " + result.ToString());
        });
    }

    // Invoked when any sign in operation has completed
    private void DidLogin(Gamer newGamer)
    {
        if (currentGamer != null)
        {
            Debug.LogWarning("Current gamer " + currentGamer.GamerId + " has been dismissed");
            Loop.Stop();
        }
        currentGamer = newGamer;
        Loop = currentGamer.StartEventLoop();
        Loop.ReceivedEvent += Loop_ReceivedEvent;
        Debug.Log("Signed in successfully (ID = " + currentGamer.GamerId + ")");
        GetProfileData();
    }

    private void Loop_ReceivedEvent(DomainEventLoop sender, EventLoopArgs e)
    {
        Debug.Log("Received event of type " + e.Message.Type + ": " + e.Message.ToJson());
    }

    public bool RequireGamer()
    {
        if (currentGamer == null)
            Debug.LogError("You need to login first. Click on a login button.");
        return currentGamer != null;
    }

    private bool RequireFriend()
    {
        if (FriendId == null)
            Debug.LogError("You need to fetch a friend first. Fill the e-mail address field and click Fetch Friend.");
        return FriendId != null;
    }

    public void GetProfileData()
    {
        currentGamer.Profile.Get()
        .Done(profileRes =>
        {
            Debug.Log("Profile data: " + profileRes.ToString());
            if (m_LoginUIManager != null)
            {
                m_LoginUIManager.Login(profileRes["displayName"]);
            }
        }, ex =>
        {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not get profile data due to error: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }

    public void DoLogout()
    {
        Cloud.Logout(currentGamer)
        .Done(result =>
        {
            Debug.Log("Logout succeeded");
        }, ex =>
        {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Failed to logout: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
        });

    }

    public void AddToLeaderboard(long score, string board)
    {
        Debug.Log(score + " " + board);
        currentGamer.Scores.Domain("private").Post(score, board, ScoreOrder.LowToHigh)
        .Done(postScoreRes =>
        {
            Debug.Log("Post score: " + postScoreRes.ToString());
        }, ex =>
        {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not post score: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }

    public void BestHighScores(string board)
    {
        currentGamer.Scores.Domain("private").BestHighScores(board, 10, 1)
        .Done(bestHighScoresRes =>
        {
            m_LeaderboardUIManager = FindObjectOfType<LeaderboardUIManager>();
            m_LeaderboardUIManager.DisplayLeaderboard(bestHighScoresRes);
        }, ex =>
        {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not get best high scores: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }


}

