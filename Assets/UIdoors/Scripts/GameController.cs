using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
namespace Assets.UIDoors

{ public class GameController : AGameController
    {
        // В нашей сцене именно GameController управляет всеми элементами. При усложнении системы его легко можно заменить на другой компонент

        // Прошу обратить внимание, что все модули (игрок, камера, игра) обращаются друг к другу через абстракции
        [SerializeField] ARoomController[] roomControllers;
        [SerializeField] ARoomController currentRoomController;
        public ARoomController CurrentRoomController
        {
            get { return currentRoomController; }
            private set { currentRoomController = value; }
        }

        [SerializeField] APlayerController playerController;
        [SerializeField] ACameraController cameraController;
        public ACameraController CameraController
        {
            get {
                if (cameraController == null)
                    //Camera.main.GetComponent<CameraController>();
                    cameraController = FindAnyObjectByType<CameraController>();//другие удобные способы получить camera из префаба?
                return  cameraController; }
            private set
            {
                cameraController = value;
            }
        }

        [SerializeField] Assets.UIDoors.GameStats gameStats;
        public class RoomRules
        {
            public bool ChestsRoomsAvailable;
            public bool NoChestsRoomsAvailable;
        }
        public ARoomController GetRandomAvailableRoomController(RoomRules roomRules = null)
        {
            ARoomController[] availableRooms = roomControllers;
            if (roomRules !=null)
            {
                if(!roomRules.ChestsRoomsAvailable)
                    availableRooms = availableRooms.Where(x => x.Room.IsChestsRoom != true).ToArray();
                if(!roomRules.NoChestsRoomsAvailable)
                    availableRooms = availableRooms.Where(x => x.Room.IsChestsRoom == true).ToArray();
            }
            availableRooms = availableRooms.Where(x => x != currentRoomController).ToArray();
            var toRoomController = availableRooms[UnityEngine.Random.Range(0, availableRooms.Length)]; // Выбор следующей комнаты

            return toRoomController;
        }
        public override void MoveToRoom(ARoomController toRoomController, int throughTheDoorNum, Action onCompleted = null)
        {
            StartCoroutine(MovingRoutine(toRoomController, throughTheDoorNum,onCompleted));
        }
        public override void MoveNextRoom(Action onCompleted = null)
        {
            ARoomController toRoomController = GetRandomAvailableRoomController();
            int throughTheDoorNum = UnityEngine.Random.Range(0, currentRoomController.Room.DoorPoints.Length); // Выбор двери, если их больше 1

            StartCoroutine(MovingRoutine(toRoomController, throughTheDoorNum, onCompleted));
        }

        IEnumerator MovingRoutine(ARoomController toRoomController, int throughTheDoorNum, Action onCompleted = null)
        {
           
            toRoomController.SetRendererSortingOrder(-1); // Поскольку все в игре работает на спрайтах, нужно учитывать их SortingOrder, чтобы они не перекрывали друг друга

            if (currentRoomController.Room.DoorPoints.Count() - 1 < throughTheDoorNum)
                throw new Exception("wrong door num");
            toRoomController.SetOnPosition(currentRoomController.Room.DoorPoints[throughTheDoorNum].position); //размещение комнаты в точке двери предыдущей

            bool completed = false; // данный флаг будет проверять выполнение определенных корутин

            currentRoomController.UnfadeDoor(throughTheDoorNum, () => { completed = true; }); // Растуманивание двери

            while (!completed)
            {
                yield return null;
            }

            completed = false;

            playerController.MoveToPosition( // Перемещение игрока к двери
                toRoomController.Room.StartPoint.position,
                () => { completed = true; }
            );

            float fullDistance = Vector3.Distance(currentRoomController.Room.StartPoint.position, currentRoomController.Room.DoorPoints[throughTheDoorNum].position);

            while (Vector3.Distance(playerController.transform.position, currentRoomController.Room.StartPoint.position) / fullDistance < gameStats.CameraMoveTreshold)
            {
                yield return null; // Ждем, пока игрок пройдет часть пути
            }
            var hidingCompleted = false;

            CameraController.MoveToPosition(
                toRoomController.Room.StartPoint.position// + gameStats.CameraDoorOffset
            ); // Перемещение камеры
            currentRoomController.HideRoom(throughTheDoorNum, () => { hidingCompleted = true; });

            while (!hidingCompleted)
            {
                yield return null;
            }

            completed = false;

            playerController.transform.position = toRoomController.Room.StartPlayerPoint.position;

            playerController.MoveToPosition(
                toRoomController.Room.CenterPoint.position,
                () => completed = true
            );

            // CameraController.MoveToPosition(
            //     toRoomController.Room.CenterPoint.position
            // );



            while (!completed || !hidingCompleted)
            {
                yield return null;
            }


            currentRoomController = toRoomController;

            currentRoomController.SetRendererSortingOrder(1);

            onCompleted?.Invoke();
        }

        internal int GetDoorIndex(ClickableObject doorToExit)
        {
            for (int i = 0;i < currentRoomController.Room.ClickableDoors.Count();i++)
            {
                if (currentRoomController.Room.ClickableDoors[i]==doorToExit)
                    return i;
            }
            return -1;
        }
    } }
