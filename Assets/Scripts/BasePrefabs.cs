using System;
using Assets.GamePlay.Effects;
using Assets.Scripts.Animations;
using Assets.Scripts.Gameplay;
using Assets.Scripts.UI;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.Utils;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.Utils;
using Gameplay.Components;
using RotaryHeart.Lib.SerializableDictionary;
using Scripts.UI.General;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
	public class BasePrefabs : MonoBehaviour
	{
		[Space]
		public Color TextGreenColor = Color.green;
		public Color TextRedColor = Color.red;
		
		public BuildSettings.BuildSettings Settings;
		public PreloaderScreen PreloaderScreen;
		[Space]
		public Material DisableMaterial;
        public Material DisableMaterialIOS;
        public Material DisableMaterialAlpha;
        public Material DefaultSpineMaterial;
        public Material TintBlackSpineMaterial;
        public Material DefaultSpineOutlineMaterial;
        public Shader DefaultSpineSkeletonShader;
        public Material DefaultSpriteMaterial;
		[Space]
        public ItemCountViewWithName DropItemCount;
		public FloatingText PrefabFloatingText;
		public FloatingText PrefabFloatingTextDark;
		
		[Space]
		[SerializeField] public Color32Dictionary UnitTypeColors;
		
		[Header("Buttons")] 
		[SerializeField] private ButtonsDictionary buttons;
		
		public ButtonsDictionary Buttons => buttons;
		
		[Header("GamePlay")]
		[SerializeField] public PlayfieldView PlayfieldPrefab;
		[SerializeField] public TMP_Text LifeChangeText;
		public Color LifeChangeCommonColor = Color.white;
		public Color LifeChangeCritColor = Color.red;
		[SerializeField] public ExplosionAnimation StanEffect;
		[SerializeField] public ParticlesDictionary UpgradeParticles;
		[SerializeField] public ParticlesDictionary MergeParticles;

	}
	
	[Serializable]
	public class ParticlesDictionary : SerializableDictionaryBase<string, ParticlesView>
	{
	}
	
	[Serializable]
	public class SpriteDictionary : SerializableDictionaryBase<string, Sprite>
	{
	}
	
	[Serializable]
	public class Color32Dictionary : SerializableDictionaryBase<string, Color32>
	{
	}
    
    [Serializable]
    public class ButtonsDictionary : SerializableDictionaryBase<ButtonPrefab, BasicButton>
    {
        public BasicButton InstantiateTextButton( ButtonPrefab prefab,
            string text, Transform parent = null)
        {
            var textButton = InstantiateButton<ButtonText>(prefab, parent);
            if (textButton) textButton.Text = text;

            return textButton;
        }

        private T InstantiateButton<T>(ButtonPrefab prefabType, Transform parentTransform = null) where T : BasicButton
        {
            TryGetValue(prefabType, out var p);
            var inst = parentTransform ? Object.Instantiate(p, parentTransform) : Object.Instantiate(p);
            return inst as T;
        }

        public BasicButton InstantiateButton(ButtonPrefab prefabType, string text = null, Sprite sprite = null, Transform parent = null)
        {
            TryGetValue(prefabType, out var prefab);
            var inst = Object.Instantiate(prefab, parent);

            if (sprite != null)
            {
                if (inst is ButtonTextIcon buttonTextIcon)
                    buttonTextIcon.Icon.sprite = sprite;

                else Debug.LogError($"Trying to create button with sprite from non icon type : {prefabType} | inst : {inst} | sprite {sprite}");
            }

            if (text != null)
            {
                if (inst is ButtonText buttonText)
                    buttonText.Text = text;

                else Debug.LogError($"Trying to create button with text from non text type : {prefabType} | inst : {inst} | text {text}");
            }

            return inst;
        }

        private BasicButton TryGet(ButtonPrefab prefabType)
        {
            return this.ContainsKey(prefabType) ? this[prefabType] : null;
        }

        public BasicButton InstantiateButton(ButtonPrefab prefabType, string text = null, string iconPath = null, Transform parent = null)
        {
            var prefab = TryGet(prefabType);
            var inst = Object.Instantiate(prefab, parent);

            if (!iconPath.IsNullOrEmpty())
            {
                if (inst is ButtonTextIcon buttonTextIcon)
                    buttonTextIcon.Icon.LoadFromAssets(iconPath);

                else Debug.LogError($"Trying to create button with sprite from non icon type : {prefabType} | inst : {inst} | sprite {iconPath}");
            }

            if (text != null)
            {
                if (inst is ButtonText buttonText)
                    buttonText.Text = text;

                else Debug.LogError($"Trying to create button with text from non text type : {prefabType} | inst : {inst} | text {text}");
            }

            return inst;
        }
    }
}