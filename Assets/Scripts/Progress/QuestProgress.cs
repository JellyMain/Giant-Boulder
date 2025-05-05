using System;
using Quests;
using Quests.Enums;


namespace Progress
{
    [Serializable]
    public class QuestProgress
    {
        public QuestState questState;
        public int collectedCoins;
        public int destroyedObjects;
    }
}
