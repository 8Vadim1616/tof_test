using System;
using JetBrains.Annotations;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
    public class TimerTitlePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textTitle;
        [SerializeField] private TextMeshProUGUI textTimer;

        private IDisposable timerUpdate;

        public event Action OnTimerEvent;

        public void Init(string title, [NotNull] Func<string> timerText, [NotNull] Func<bool> timerEnded, Action onEndedTimer = null, float checkIntervalSeconds = 1f)
        {
            textTitle.text = title;

            timerUpdate?.Dispose();
            timerUpdate = Observable.Interval(TimeSpan.FromSeconds(checkIntervalSeconds))
                .StartWith(0)
                .Subscribe(_ =>
                {
                    textTimer.text = timerText.Invoke();
                    OnTimerEvent?.Invoke();

                    if (!timerEnded.Invoke()) return;
                    timerUpdate?.Dispose();
                    onEndedTimer?.Invoke();
                })
                .AddTo(this);
        }
    }
}
