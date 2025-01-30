using UniRx;

namespace Gameplay
{
    public class TowerState
    {
        // Реактивное свойство, которое отслеживает, нужно ли отображать кнопку
        public ReactiveProperty<bool> ShouldShowButton = new ReactiveProperty<bool>(false);        
    }
}