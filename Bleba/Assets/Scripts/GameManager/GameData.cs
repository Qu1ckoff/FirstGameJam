using UnityEngine;

[System.Serializable]
public class GameData
{
    public string sceneName;
    public float[] playerPosition;

    public GameData(string sceneName, Vector3 playerPos)
    {
        this.sceneName = sceneName;
        playerPosition = new float[3] { playerPos.x, playerPos.y, playerPos.z };
    }

    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerPosition[0], playerPosition[1], playerPosition[2]);
    }
}
