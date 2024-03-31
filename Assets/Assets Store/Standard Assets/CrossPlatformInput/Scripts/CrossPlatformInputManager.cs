using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput.PlatformSpecific;

namespace UnityStandardAssets.CrossPlatformInput
{
	public static class CrossPlatformInputManager
	{
		public enum ActiveInputMethod
		{
			Hardware,
			Touch
		}


		static VirtualInput activeInput;

		static VirtualInput s_TouchInput;
		static VirtualInput s_HardwareInput;


		static CrossPlatformInputManager()
		{
			s_TouchInput = new MobileInput();
			s_HardwareInput = new StandaloneInput();
#if MOBILE_INPUT
            activeInput = s_TouchInput;
#else
			activeInput = s_HardwareInput;
#endif
		}

		public static void SwitchActiveInputMethod(ActiveInputMethod activeInputMethod)
		{
			switch (activeInputMethod)
			{
				case ActiveInputMethod.Hardware:
					activeInput = s_HardwareInput;
					break;

				case ActiveInputMethod.Touch:
					activeInput = s_TouchInput;
					break;
			}
		}

		public static bool AxisExists(string name)
		{
			return activeInput.AxisExists(name);
		}

		public static bool ButtonExists(string name)
		{
			return activeInput.ButtonExists(name);
		}

		public static void RegisterVirtualAxis(VirtualAxis axis)
		{
			activeInput.RegisterVirtualAxis(axis);
		}


		public static void RegisterVirtualButton(VirtualButton button)
		{
			activeInput.RegisterVirtualButton(button);
		}


		public static void UnRegisterVirtualAxis(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			activeInput.UnRegisterVirtualAxis(name);
		}


		public static void UnRegisterVirtualButton(string name)
		{
			activeInput.UnRegisterVirtualButton(name);
		}



        // �w�肳�ꂽ���O�̉��z���ւ̎Q�Ƃ�Ԃ��܂��B���݂��Ȃ��ꍇ��null��Ԃ��܂��B
        public static VirtualAxis VirtualAxisReference(string name)
		{
			return activeInput.VirtualAxisReference(name);
		}


        // �w�肳�ꂽ���O�ɑ΂��ăv���b�g�t�H�[���ɓK��������Ԃ��܂��B
        public static float GetAxis(string name)
		{
			return GetAxis(name, false);
		}


		public static float GetAxisRaw(string name)
		{
			return GetAxis(name, true);
		}



        // ���̊֐��́A�����̃^�C�v�̎��i���iraw�j�Ɣ񐶁inot raw�j�j�������܂��B
        static float GetAxis(string name, bool raw)
		{
			return activeInput.GetAxis(name, raw);
		}



        // -- �{�^������ --
        public static bool GetButton(string name)
		{
			return activeInput.GetButton(name);
		}


		public static bool GetButtonDown(string name)
		{
			return activeInput.GetButtonDown(name);
		}


		public static bool GetButtonUp(string name)
		{
			return activeInput.GetButtonUp(name);
		}


		public static void SetButtonDown(string name)
		{
			activeInput.SetButtonDown(name);
		}


		public static void SetButtonUp(string name)
		{
			activeInput.SetButtonUp(name);
		}


		public static void SetAxisPositive(string name)
		{
			activeInput.SetAxisPositive(name);
		}


		public static void SetAxisNegative(string name)
		{
			activeInput.SetAxisNegative(name);
		}


		public static void SetAxisZero(string name)
		{
			activeInput.SetAxisZero(name);
		}


		public static void SetAxis(string name, float value)
		{
			activeInput.SetAxis(name, value);
		}


		public static Vector3 mousePosition
		{
			get { return activeInput.MousePosition(); }
		}


		public static void SetVirtualMousePositionX(float f)
		{
			activeInput.SetVirtualMousePositionX(f);
		}


		public static void SetVirtualMousePositionY(float f)
		{
			activeInput.SetVirtualMousePositionY(f);
		}


		public static void SetVirtualMousePositionZ(float f)
		{
			activeInput.SetVirtualMousePositionZ(f);
		}


        // ���z���ƃ{�^���N���X - ���o�C�����͂ɓK�p����܂��B
        // �^�b�`�W���C�X�e�B�b�N�A�X���A�W���C���ȂǂɃ}�b�s���O�ł��A�����ɉ����ĈقȂ�܂��B
        // �܂��A���̓��̓f�o�C�X�i�L�l�N�g�A�d�q�Z���T�[�Ȃǁj�ɂ���Ă����������\��������܂��B
        public class VirtualAxis
		{
			public string name { get; set; }
			float m_Value;
			public bool matchWithInputManager { get; set; }


			public VirtualAxis(string name)
				: this(name, true)
			{
			}


			public VirtualAxis(string name, bool matchToInputSettings)
			{
				this.name = name;
				matchWithInputManager = matchToInputSettings;
			}


			// removes an axes from the cross platform input system
			public void Remove()
			{
				UnRegisterVirtualAxis(name);
			}


			// a controller gameobject (eg. a virtual thumbstick) should update this class
			public void Update(float value)
			{
				m_Value = value;
			}


			public float GetValue
			{
				get { return m_Value; }
			}


			public float GetValueRaw
			{
				get { return m_Value; }
			}
		}

		// a controller gameobject (eg. a virtual GUI button) should call the
		// 'pressed' function of this class. Other objects can then read the
		// Get/Down/Up state of this button.
		public class VirtualButton
		{
			public string name { get; set; }
			public bool matchWithInputManager { get; set; }

			int m_LastPressedFrame = -5;
			int m_ReleasedFrame = -5;
			bool m_Pressed;


			public VirtualButton(string name)
				: this(name, true)
			{
			}


			public VirtualButton(string name, bool matchToInputSettings)
			{
				this.name = name;
				matchWithInputManager = matchToInputSettings;
			}


			// A controller gameobject should call this function when the button is pressed down
			public void Pressed()
			{
				if (m_Pressed)
				{
					return;
				}
				m_Pressed = true;
				m_LastPressedFrame = Time.frameCount;
			}


			// A controller gameobject should call this function when the button is released
			public void Released()
			{
				m_Pressed = false;
				m_ReleasedFrame = Time.frameCount;
			}


			// the controller gameobject should call Remove when the button is destroyed or disabled
			public void Remove()
			{
				UnRegisterVirtualButton(name);
			}


			// these are the states of the button which can be read via the cross platform input system
			public bool GetButton
			{
				get { return m_Pressed; }
			}


			public bool GetButtonDown
			{
				get
				{
					return m_LastPressedFrame - Time.frameCount == -1;
				}
			}


			public bool GetButtonUp
			{
				get
				{
					return (m_ReleasedFrame == Time.frameCount - 1);
				}
			}
		}
	}
}
