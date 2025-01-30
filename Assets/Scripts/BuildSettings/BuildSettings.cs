using UnityEngine;

namespace Assets.Scripts.BuildSettings
{
    public class BuildSettings : ScriptableObject
    {
        public static BuildSettings Instance { get; private set; }
        private void OnEnable() => Instance = this;

        [SerializeField] private ServerTypeEnum serverType = ServerTypeEnum.Release;
        [SerializeField] private GameTypeEnum gameType = GameTypeEnum.Game;
        [SerializeField] private int modelGroup = 99;
		[SerializeField] private bool isUsingObb;
		[SerializeField] private bool sendMailOnBuildSuccess = true;

        public static ServerTypeEnum ServerType
        {
            get  => Instance.serverType;
            set  => Instance.serverType = value;
        }

        public static GameTypeEnum GameType
        {
            get  => Instance.gameType;
            set  => Instance.gameType = value;
        }
        
        public static int ModelGroup => Instance.modelGroup;

		public static bool IsUsingObb
		{
			get => Instance.isUsingObb;
			set => Instance.isUsingObb = value;
		}

		public static bool SendMailOnBuildSuccess
		{
			get => Instance.sendMailOnBuildSuccess;
			set => Instance.sendMailOnBuildSuccess = value;
		}
        
        public static bool IsTest => ServerType == ServerTypeEnum.Test;
        //public static bool IsPredproduction => ServerType == ServerTypeEnum.Predproduction;
        public static bool IsRelease => ServerType == ServerTypeEnum.Release;
        
        public static bool IsEditor => GameType == GameTypeEnum.Editor;
    }

    public enum ServerTypeEnum
    {
        Release = 1,
        Test = 2,
        //Predproduction = 3
    }

    public enum GameTypeEnum
    {
        Game = 1,
        Editor = 2,
    }
}