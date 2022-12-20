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


    public Packet(int newId)
    {
        buffer = new List<byte>();
        readPos = 0;

        Write(newId);
    }


    public Packet(byte[] newData)
    {
        buffer = new List<byte>();
        readPos = 0;

        SetBytes(newData);
    }


    public void SetBytes(byte[] newData)
    {
        Write(newData);
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

    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
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

    public void Write(Vector3 vector)
    {
        Write(vector.x);
        Write(vector.y);
        Write(vector.z);
    }

    public void Write(Quaternion quaternion)
    {
        Write(quaternion.x);
        Write(quaternion.y);
        Write(quaternion.z);
        Write(quaternion.w);
    }

    public byte ReadByte(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            byte _value = readableBuffer[readPos];
            if (moveReadPos)
            {
                readPos += 1;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte'!");
        }
    }


    public byte[] ReadBytes(int newLength, bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {

            byte[] newValue = buffer.GetRange(readPos, newLength).ToArray();
            if (moveReadPos)
            {
                readPos += newLength;
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
            int length = ReadInt();
            string newValue = Encoding.ASCII.GetString(readableBuffer, readPos, length);
            if (moveReadPos && newValue.Length > 0)
            {
                readPos += length;
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

    public Quaternion ReadQuaternion(bool rotateReadPos = true)
    {
        return new Quaternion(ReadFloat(rotateReadPos), ReadFloat(rotateReadPos), ReadFloat(rotateReadPos), ReadFloat(rotateReadPos));
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

