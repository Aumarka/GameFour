using DunGen.Demo;
using UnityEngine;

namespace DunGen.DungeonCrawler
{
	sealed class PlayerInput : MonoBehaviour
	{
		[SerializeField]
		private float clickRepeatInterval = 0.5f;

		[SerializeField]
		private ClickToMove movement = null;
		[SerializeField]
		private ClickableObjectHandler clickableObjectHandler = null;
		[SerializeField]
		private Camera playerCamera = null;

		private float lastClickTime;
		private DemoInputBridge inputBridge;


		private void Awake()
		{
			inputBridge = new DemoInputBridge();
		}

		private void Update()
		{
			if (inputBridge.GetExit())
				ExitDemo();

			var leftMouseState = inputBridge.GetLeftMouseInputState();

			if (leftMouseState == InputState.Pressed)
				clickableObjectHandler.Click();
			else if (leftMouseState == InputState.Held)
			{
				if (clickableObjectHandler.HoverClickable == null &&
					Time.time >= lastClickTime + clickRepeatInterval)
					MoveToCursor();
			}
			else if (leftMouseState == InputState.Released)
				movement.StopManualMovement();
		}

		private void MoveToCursor()
		{
			bool newlyPressed = inputBridge.GetLeftMouseInputState() == InputState.Pressed;
			var cursorRay = playerCamera.ScreenPointToRay(inputBridge.GetMousePosition());
			movement.Click(cursorRay, newlyPressed);

			lastClickTime = Time.time;
		}

		private void ExitDemo()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}
