using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;

public class UIHudScript : MonoBehaviour
{
    public UIDocument UIDocument;
    //public CarControllerScript carControllerScript;
    public PrometeoCarController2 betterCarControllerScript;

    private Label speedLabel, gearLabel, rpmLabel;

    public LocalizedString speedText;
    public LocalizedString gearText;
    public LocalizedString rpmText;

    void Start()
    {
        var root = UIDocument.rootVisualElement;

        speedLabel = root.Q<Label>("SpeedLabel");
        gearLabel = root.Q<Label>("GearLabel");
        rpmLabel = root.Q<Label>("RPMLabel");
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (betterCarControllerScript == null)
        {
            Debug.Log("BRAK CAR CONTROLLERA");
            return;
        }

        float speed = 3.6f * betterCarControllerScript.ReturnCurrentSpeed();
        //int gear = carControllerScript.ReturnCurrentGear() - 2;
        //float rpm = carControllerScript.ReturnCurrenRPM();

        speedText.Arguments = new object[] { speed.ToString("F0") };
        //gearText.Arguments = new object[] { gear };
        //rpmText.Arguments = new object[] { rpm.ToString("F0") };

        speedLabel.text = speedText.GetLocalizedString();
        gearLabel.text = gearText.GetLocalizedString();
        rpmLabel.text = rpmText.GetLocalizedString();
    }
}