using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;


public enum ServerPackets
{
    welcome = 1,
    spawnPlayer,
    playerPos,
    playerRot
}


public enum ClientPackets
{
    welcomeReceived = 1,
    playerMove
}

public class Packet : IDisposable
{
    
    

    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos;


    public Packet()
    {
        buffer = new List<byte>();
        readPos = 0;
    }

    public Packet(int _id)
    {
        buffer = new List<byte>();
        readPos = 0;

        Write(_id);
    }


    public Packet(byte[] _data)
    {
        buffer = new List<byte>();
        readPos = 0;

        SetBytes(_data);
    }


    public void SetBytes(byte[] _data)
    {
        Write(_data);
        readableBuffer = buffer.ToArray();
    }

    public void WriteLength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
    }

    public void InsertInt(int newValue)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(newValue));
    }


    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }


    public int Length()
    {
        return buffer.Count;
    }


    public int UnreadLength()
    {
        return Length() - readPos;
    }


    public void Reset(bool _shouldReset = true)
    {
        if (_shouldReset)
        {
            buffer.Clear();
            readableBuffer = null;
            readPos = 0;
        }
        else
        {
            readPos -= 4;
        }
    }

    public void Write(byte newValue)
    {
        buffer.Add(newValue);
    }

    public void Write(byte[] newValue)
    {
        buffer.AddRange(newValue);
    }

    public void Write(short newValue)
    {
        buffer.AddRange(BitConverter.GetBytes(newValue));
    }

    public void Write(int newValue)
    {
        buffer.AddRange(BitConverter.GetBytes(newValue));
    }

    public void Write(long newValue)
    {
        buffer.AddRange(BitConverter.GetBytes(newValue));
    }

    public void Write(float newValue)
    {
        buffer.AddRange(BitConverter.GetBytes(newValue));
    }

    public void Write(bool newValue)
    {
        buffer.AddRange(BitConverter.GetBytes(newValue));
    }

    public void Write(string newValue)
    {
        Write(newValue.Length);
        buffer.AddRange(Encoding.ASCII.GetBytes(newValue));
    }

    public void Write(Vector3 newValue)
    {
        Write(newValue.x);
        Write(newValue.y);
        Write(newValue.z);
    }

    public void Write(Quaternion newValue)
    {
        Write(newValue.x);
        Write(newValue.y);
        Write(newValue.z);
        Write(newValue.w);
    }

    public byte ReadByte(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {

            byte newValue = readableBuffer[readPos];
            if (moveReadPos)
            {
                readPos += 1;
            }
            return newValue;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte'!");
        }
    }

    public byte[] ReadBytes(int _length, bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {

            byte[] newValue = buffer.GetRange(readPos, _length).ToArray();
            if (moveReadPos)
            {
                readPos += _length;
            }
            return newValue;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }

    public short ReadShort(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            short newValue = BitConverter.ToInt16(readableBuffer, readPos);
            if (moveReadPos)
            {
                readPos += 2;
            }
            return newValue;
        }
        else
        {
            throw new Exception("Could not read value of type 'short'!");
        }
    }

    public int ReadInt(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {

            int newValue = BitConverter.ToInt32(readableBuffer, readPos);
            if (moveReadPos)
            {
                readPos += 4;
            }
            return newValue;
        }
        else
        {
            throw new Exception("Could not read value of type 'int'!");
        }
    }

    public long ReadLong(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {

            long newValue = BitConverter.ToInt64(readableBuffer, readPos);
            if (moveReadPos)
            {
                readPos += 8;
            }
            return newValue;
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }
    public float ReadFloat(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {

            float newValue = BitConverter.ToSingle(readableBuffer, readPos);
            if (moveReadPos)
            {
                readPos += 4;
            }
            return newValue;
        }
        else
        {
            throw new Exception("Could not read value of type 'float'!");
        }
    }


    public bool ReadBool(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            bool newValue = BitConverter.ToBoolean(readableBuffer, readPos);
            if (moveReadPos)
            {
                readPos += 1;
            }
            return newValue;
        }
        else
        {
            throw new Exception("Could not read value of type 'bool'!");
        }
    }
    public string ReadString(bool moveReadPos = true)
    {
        try
        {
            int _length = ReadInt();
            string newValue = Encoding.ASCII.GetString(readableBuffer, readPos, _length);
            if (moveReadPos && newValue.Length > 0)
            {
                readPos += _length;
            }
            return newValue;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    public Vector3 ReadVector3(bool moveReadPos = true)
    {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    public Quaternion ReadQuaternion(bool moveReadPos = true)
    {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    private bool disposed = false;

    protected virtual void Dispose(bool _disposing)
    {
        if (!disposed)
        {
            if (_disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
