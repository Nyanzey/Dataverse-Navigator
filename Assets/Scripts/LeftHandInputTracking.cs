using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class LeftHandInputTracking : MonoBehaviour
{
    //Raycast tracking variables
    private Transform highlight;
    private RaycastHit raycastHit;
    Utils utilsScript;
    public XRRayInteractor rayInteractor;

    public GameObject MenuCanvas;
    public GameObject OptionCanvas;
	public AudioSource SFX;
    private bool canOpenMenu = true;

    // Speed at which selected points are expanded or condensed
    public float expansionSpeed = 1.0f;

    // Helper variable to control button presses
    private bool wasPrimaryButtonRightDown = false;

    // Start is called before the first frame update
    void Start()
    {
        utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();
    }

    // Update is called once per frame
    private void Update()
    {
        List<InputDevice> m_deviceleft = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, m_deviceleft);
        if (m_deviceleft.Count == 1)
        {
            // Debug.Log("One left device found");
            CheckControllerLeft(m_deviceleft[0]);
        }

        List<InputDevice> m_deviceright = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, m_deviceright);
        if (m_deviceright.Count == 1)
        {
            // Debug.Log("One right device found");
            CheckControllerRight(m_deviceright[0]);
        }

        // For highlighting objects on hover
        CheckControllerXR();
    }

    //Check objects pointed to raycast
    void CheckControllerXR()
    {
        if (highlight != null)
        {
            utilsScript.disableOutline(highlight.gameObject);
            highlight = null;
        }

        if (rayInteractor && rayInteractor.TryGetCurrent3DRaycastHit(out raycastHit))
        {
            highlight = raycastHit.transform;
            if (highlight.CompareTag("SelectablePoint"))
            {
                utilsScript.enableOutline(highlight.gameObject);
            }
            else
            {
                highlight = null;
            }
        }
    }

    //Check controller buttons
    private void CheckControllerLeft(InputDevice device)
    {
        // Menu
        bool menuButtonDown = false;
        device.TryGetFeatureValue(CommonUsages.menuButton, out menuButtonDown);
        if (menuButtonDown && canOpenMenu)
        {
			if (!MenuCanvas.activeSelf)
				SFX.PlayOneShot(Resources.Load<AudioClip>("ui_open"), 1.0f);
			else
				SFX.PlayOneShot(Resources.Load<AudioClip>("ui_close"), 1.0f);
			
            Debug.Log("Menu pressed");
            MenuCanvas.SetActive(!MenuCanvas.activeSelf);
            OptionCanvas.SetActive(false);
            canOpenMenu = false;
            StartCoroutine(setOpenMenu());
        }

        // Primary and secondary buttons
        bool primaryButtonDown = false, secondaryButtonDown = false;
        device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonDown);
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonDown);

        GroupSelectorScript sphereSelector = GameObject.FindWithTag("sphereSelector")?.GetComponent<GroupSelectorScript>();
        if (sphereSelector != null)
        {
            if (primaryButtonDown)
            {
                Debug.Log("Primary button (left) is pressed");
                sphereSelector.scaleArea(-1);
            }
            else if (secondaryButtonDown)
            {
                Debug.Log("Secondary button (left) is pressed");
                sphereSelector.scaleArea(1);
            }
        }
        else
        {
            if (primaryButtonDown)
            {
                Debug.Log("Primary button (left) is pressed");
                utilsScript.compressSelected(expansionSpeed, true, utilsScript.getClosestSpaceIdToPlayer());
            }
            else if (secondaryButtonDown)
            {
                Debug.Log("Secondary button (left) is pressed");
                utilsScript.compressSelected(expansionSpeed, false, utilsScript.getClosestSpaceIdToPlayer());
            }
        }
    }

    private void CheckControllerRight(InputDevice device)
    {
        bool primaryButtonDown = false;
        device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonDown);

        // Check for transition from not pressed to pressed
        if (primaryButtonDown && !wasPrimaryButtonRightDown)
        {
            Debug.Log("Primary button (right) is pressed");
            Vector3 newPos = utilsScript.getNextSpacePositionToTeleport();
            GameObject VRRig = GameObject.FindGameObjectWithTag("originrig");
            if (VRRig)
            {
                Debug.Log("Found VR Rig");
                VRRig.transform.position = newPos;
            }
        }

        // Update the previous button state
        wasPrimaryButtonRightDown = primaryButtonDown;
    }

    private IEnumerator setOpenMenu() {
        yield return new WaitForSeconds(0.25f);
        canOpenMenu = true;
    }
}
