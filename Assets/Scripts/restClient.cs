using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

// Define a serializable class for user data
[System.Serializable]
public class User
{
    // User properties
    public int id;
    public string username;
    public string password;
    public string firstname;
    public string lastname;
    public string created;
    public string lastseen;
    public int banned;
    public int isadmin;
    public int score;

    // Create a User instance from JSON string
    public static User CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<User>(jsonString);
    }

}

// Define a serializable class for thread data
[System.Serializable]
public class Thread
{
    // Thread properties
    public int id;
    public string title;
    public int author;
    public string created;
    public int hidden;

    // Create a Thread instance from JSON string
    public static Thread CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Thread>(jsonString);
    }

}

// Define a serializable class for message data
[System.Serializable]
public class Msg
{
    // Message properties
    public int id;
    public int thread;
    public string title;
    public string content;
    public int author;
    public int replyto;
    public string created;
    public string modified;
    public int hidden;

    // Create a Msg instance from JSON string
    public static Msg CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Msg>(jsonString);
    }

}

// Helper class for working with JSON arrays
public static class JsonHelper
{
    // Deserialize a JSON array into an array of objects
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    // Serialize an array of objects into a JSON string
    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    // Serialize an array of objects into a formatted (pretty-printed) JSON string
    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    // Wrapper class to hold an array of items during serialization/deserialization
    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

// Main class for the REST client behavior
public class restClient : MonoBehaviour
{
    // Base URL for the API
    private string baseurl = "http://localhost:80/bb/api";

    // Flag indicating whether the user is logged in
    private bool loggedIn = false;

    // UI elements
    public TextMeshProUGUI login;
    public TextMeshProUGUI password;
    public TextMeshProUGUI playerDisplay;
    public TextMeshProUGUI scoreDisplay;
    public TextMeshProUGUI UsersShow;
    public TextMeshProUGUI ThreadsShow;
    public TextMeshProUGUI MsgsShow;

    // User-specific data
    public string username;
    public int id;
    public User[] users;
    public Thread[] threads;
    public Msg[] msgs; 

    // Start is called before the first frame update
    void Start()
    {
        // Start polling users and messages when the scene begins
        StartCoroutine(PollUsers());
        StartCoroutine(PollMsgs());
    }

    // Fix JSON format (add "Items" field) before deserialization
    private string fixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    // Coroutine for handling user login
    IEnumerator Login(string username, string password)
    {
        // Send a login request to the server
        UnityWebRequest www = UnityWebRequest.Post(baseurl + "/login", "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }", "application/json");
        yield return www.SendWebRequest();

        // Check if the login request was successful
        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            Debug.Log(baseurl + "/login");
            Debug.Log(www.error);
            Debug.Log(text);
        }
        else
        {
            // Log in successful
            Debug.Log("Login complete!");
            loggedIn = true;
            var text = www.downloadHandler.text;
            Debug.Log(text);
            playerDisplay.text = "Player: " + username;
        }
    }

    // Button click event for logging in
    public void LogButton()
    {
        // When getting string from input, it adds invisible special character, so we have to clean it
        string sanitizedUsername = login.text.Replace("\u200B", "");
        string sanitizedPassword = password.text.Replace("\u200B", "");
        StartCoroutine(Login(sanitizedUsername, sanitizedPassword));
    }

    // Button click event for checking events
    public void Forums()
    {
        StartCoroutine(PollThreads());
    }

       //Poistettu käytöstä toimivuuden takia // oli kuitenkin liian hyvä poistaakseni kokonaan
       //Tarkoituksena oli saada pelaajien tulokset päivitetty
      public void CallSaveData()
      {
          StartCoroutine(SavePlayerData(1));
      }

      IEnumerator SavePlayerData(int playerID)
      {
          UnityWebRequest www = UnityWebRequest.Put(baseurl + "/users/score/" + playerID, "{ \"score\": \"1000\"}");
          www.SetRequestHeader("Content-Type", "application/json");
          yield return www.SendWebRequest();

      }

    // Coroutine for fetching and displaying forum threads
    IEnumerator PollThreads()
    {
        // Wait for the user to log in before polling threads
        while (!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        // Continuously poll threads every 5 seconds
        while (true)
        {
            // Send a request to get the list of threads from the server
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/threads");
            yield return www.SendWebRequest();

            // Check if the request was successful
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl + "/threads");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                // Parse the JSON response and update the UI
                var text = www.downloadHandler.text;
                Debug.Log("threads download complete: " + text);
                loggedIn = true;
                string jsonString = fixJson(text);
                threads = JsonHelper.FromJson<Thread>(jsonString);

                for (int i = 0; i < threads.Length; i++)
                {
                    ThreadsShow.text += "Title: " + threads[i].title + " || ID: " + threads[i].id + " || Author: " + threads[i].author + "\n";
                    Debug.Log("Showing threads!");
                }

            }
            // Wait for 5 seconds before polling again
            yield return new WaitForSeconds(5);
        }

    }

    // Coroutine for polling user data
    IEnumerator PollUsers()
    {
        // Wait for the user to log in before polling user data
        while (!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        // Continuously poll user data every 60 seconds
        while (true)
        {
            // Send a request to get the list of users from the server
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/users");
            yield return www.SendWebRequest();

            // Check if the request was successful
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl + "/users");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                // Parse the JSON response and update the UI
                var text = www.downloadHandler.text;
                Debug.Log("users download complete: " + text);
                loggedIn = true;
                string jsonString = fixJson(text);
                users = JsonHelper.FromJson<User>(jsonString);
                
                for (int i = 0; i < users.Length; i++)
                {
                    UsersShow.text += "Players: " + users[i].username + " ID: " + users[i].id + " // ";
                }

            }
            // Wait for 60 seconds before polling again
            yield return new WaitForSeconds(60);
        }

    }

    // Coroutine for polling message data
    IEnumerator PollMsgs()
    {
        // Wait for the user to log in before polling message data
        while (!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        // Continuously poll message data every 5 seconds
        while (true)
        {
            // Send a request to get the list of messages from the server
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/msgs");
            yield return www.SendWebRequest();

            // Check if the request was successful
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl + "/msgs");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                // Parse the JSON response and update the UI
                var text = www.downloadHandler.text;
                Debug.Log("msgs download complete: " + text);
                loggedIn = true;
                string jsonString = fixJson(text);
                msgs = JsonHelper.FromJson<Msg>(jsonString);

                for (int i = 0; i < msgs.Length; i++)
                {
                    MsgsShow.text += "Title: " + msgs[i].title + " || ID: " + msgs[i].id + " || Author: " + msgs[i].author + " || Content: " + msgs[i].content + "\n";
                }
            }
            // Wait for 5 seconds before polling again
            yield return new WaitForSeconds(5);
        }
    }
}
