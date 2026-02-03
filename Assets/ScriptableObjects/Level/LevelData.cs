using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName; //has to match the scene name
    public int wavesToWin;
    public int startingGolds;
    public int startingLives;

    public int levelIndex;


    //public AudioClip backgroundMusic;
}