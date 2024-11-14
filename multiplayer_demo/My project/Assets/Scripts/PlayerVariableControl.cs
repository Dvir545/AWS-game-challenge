using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct MyStruct : INetworkSerializable
{
    public int N;
    public bool B;
    public FixedString32Bytes S;  // network variable can only use value types (string is reference type), so FixedStringXBytes allows up to X chars.
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref N);
        serializer.SerializeValue(ref B);
        serializer.SerializeValue(ref S);
    }
}
    
public class PlayerVariableControl : NetworkBehaviour
{
    private NetworkVariable<MyStruct> _myStruct = new(
        new MyStruct {N=1, B=true, S=""}, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        _myStruct.OnValueChanged += (value, newValue) => Debug.Log(OwnerClientId + "; " + newValue.N  + "; " + newValue.B + "; " + newValue.S);
    }

    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            _myStruct.Value = new MyStruct { N = Random.Range(0, 100), B = !_myStruct.Value.B, S = "Hello world!" };
        }
    }
}