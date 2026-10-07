using UnityEngine;

namespace SandGameplay.Core {
    /// <summary>
    /// Tracks persistent level completion progress via PlayerPrefs.
    /// Used by the Main Menu level select grid to visually display cleared levels.
    /// </summary>
    public static class LevelProgress {
        private const string ClearedKeyPrefix = "SandGameplay_LevelCleared_";
        private const string HighestClearedKey = "SandGameplay_HighestCleared";

        public static bool IsLevelCleared(int levelIndex) {
            return PlayerPrefs.GetInt(ClearedKeyPrefix + levelIndex, 0) == 1;
        }

        public static void SetLevelCleared(int levelIndex) {
            PlayerPrefs.SetInt(ClearedKeyPrefix + levelIndex, 1);
            int highest = PlayerPrefs.GetInt(HighestClearedKey, -1);
            if (levelIndex > highest) {
                PlayerPrefs.SetInt(HighestClearedKey, levelIndex);
            }
            PlayerPrefs.Save();
            Debug.Log($"[LevelProgress] Level {levelIndex + 1} marked as CLEARED.");
        }

        public static int GetHighestCleared() {
            return PlayerPrefs.GetInt(HighestClearedKey, -1);
        }

        public static void ResetProgress() {
            for (int i = 0; i < 50; i++) {
                PlayerPrefs.DeleteKey(ClearedKeyPrefix + i);
            }
            PlayerPrefs.DeleteKey(HighestClearedKey);
            PlayerPrefs.Save();
            Debug.Log("[LevelProgress] Progress reset.");
        }
    }
}
