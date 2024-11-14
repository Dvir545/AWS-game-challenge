using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using ClientRpcSendParams = Unity.Netcode.ClientRpcSendParams;


// RPCs - Remote Procedural Calls
// Basically functions that run only on the server
// So clients can pass data to the server and make the server execute stuff.
// This is very useful for operation that we do not want the clients to execute locally, but to get permission from the server
// e.g if a player wants to buy a crop, to avoid race conditions, it will ask the server to buy from him if there are enough crops.
// To send a message back from the server to the client,, use ClientRpc instead of ServerRpc.
public class RunningRPCs : NetworkBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SendMsgServerRpc("Hello There.");
            SendParamsServerRpc(new ServerRpcParams());
            GetResultsClientRpc(new ClientRpcParams
            {
                Send=new ClientRpcSendParams
                {
                    TargetClientIds = new List<ulong> {OwnerClientId}
                }
            });
        }
    }

    [ServerRpc]
    private void SendMsgServerRpc(string message)  // can only except value types - but string is an exception
    {
        Debug.Log("TestServerRpc From " + OwnerClientId + "; " + message);  // this will be shown only on the server!
    }
    
    
    [ServerRpc]
    private void SendParamsServerRpc(ServerRpcParams  rpcParams)
    {
        Debug.Log("TestServerRpc From " + OwnerClientId + "; " + rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void GetResultsClientRpc(ClientRpcParams rpcParams)
    {
        Debug.Log("TestClientRpc To " + OwnerClientId + "; " + rpcParams.Receive);
    }
}
