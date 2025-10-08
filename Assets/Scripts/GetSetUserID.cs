using UnityEngine;
using Unity.Netcode;

public class GetSetUserID : NetworkBehaviour
{
    private string userID;

    // Set the user ID
    public void SetUserId(string uid)
    {
        userID = uid;
        Debug.Log("User Id on spawn point" + userID);
    }

    // Get the user ID
    public string GetUserId()
    {
        try
        {
            Debug.Log("Here is your User Id "+userID);
            return userID;
        }
        catch
        {
            return "NA";
        }
    }
}
