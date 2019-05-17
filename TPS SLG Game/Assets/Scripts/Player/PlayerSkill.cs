using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour {

    public GameObject NeedleTrap;
    public GameObject IceTrap;
    public float range = 100f;
    public Text skillPointText;
    public Text missileLevelUpRow;
    public Text needleLevelUpRow;
    public Text iceLevelUpRow;
    public float levelRowSpeed = 5f;


    private bool inNeedle;
    private bool inIce;
    int shootableMask;
    Ray shootRay;
    bool canPut = false;

    GameObject tempNeedleTrap;
    GameObject tempIceTrap;
    GameObject shootingPivot;
    Material baseShaderMaterial;
    Material needleShaderMaterial;

    NetworkHost networkHost;

    private void Awake()
    {
        inNeedle = false;
        inIce = false;
        shootableMask = LayerMask.GetMask("Shootable");
        tempNeedleTrap = null;
        tempIceTrap = null;
        shootRay = new Ray();

        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "ShootingPivot")
            {
                shootingPivot = child.gameObject;
                break;
            }
        }

        networkHost = NetworkHost.GetInstance();
    }


    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        bool canPutState = (!inIce) && (!inNeedle) && (GameSettings.playerMoney >= GameSettings.TRAP_MONEY);

        if (PlayerInput.GetQ())
        {
            HandleQ(canPutState);
        }

        if (PlayerInput.GetE())
        {
            HandleE(canPutState);
        }

        MoveTempTrap();

        // send put trap msg
        if (PlayerInput.GetShirt())
        {
            HandleShift();
        }

        int skillPoint = GameSettings.playerLevel * 2 - GameSettings.iceTrapLevel - GameSettings.needleTrapLevel
            - GameSettings.missileLevel + 3;

        if (PlayerInput.GetOne())
        {
            if(skillPoint > 0 && GameSettings.iceTrapLevel < 10)
            {
                skillPoint -= 1;
                GameSettings.iceTrapLevel += 1;
                SendSkillUpMsg(GameSettings.ICE_TRAP_LEVEL_UP);
            }
        }

        if (PlayerInput.GetTwo())
        {
            if(skillPoint > 0 && GameSettings.needleTrapLevel < 10)
            {
                skillPoint -= 1;
                GameSettings.needleTrapLevel += 1;
                SendSkillUpMsg(GameSettings.NEEDLE_TRAP_LEVEL_UP);
            }
        }

        if (PlayerInput.GetThree())
        {
            if (skillPoint > 0 && GameSettings.missileLevel < 10)
            {
                skillPoint -= 1;
                GameSettings.missileLevel += 1;
                SendSkillUpMsg(GameSettings.MISSILE_LEVEL_UP);
            }
        }
        ChangeSkillText(skillPoint);
    }

    void SendSkillUpMsg(int skillType)
    {
        ClientSkillLevelUpMsg skillLevelUpMsg = new ClientSkillLevelUpMsg();
        skillLevelUpMsg.skillType = skillType;
        skillLevelUpMsg.playerID = GameSettings.playerID;

        string skillLevelUpMsgJson = JsonConvert.SerializeObject(skillLevelUpMsg);

        byte[] msg = MessageHandler.SetClientMsg(
           NetworkSettings.PLAYER_ENTITY_SERVICE_ID,
           NetworkSettings.PLAYER_SKILL_LEVEL_UP_CMD,
           skillLevelUpMsgJson);

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

    void ChangeSkillText(int skillPoint)
    {

        skillPointText.text = "Skill Point: " + skillPoint;

        missileLevelUpRow.text = "LV " + GameSettings.missileLevel;
        needleLevelUpRow.text = "LV " + GameSettings.needleTrapLevel;
        iceLevelUpRow.text = "LV " + GameSettings.iceTrapLevel;
    }

    void HandleQ(bool canPutState)
    {
        if (canPutState)
        {
            inIce = !inIce;
            TryToPutIce();
        }
        else
        {
            // cancel trap
            if (inIce)
            {
                Destroy(tempIceTrap);
                tempIceTrap = null;
                inIce = !inIce;
            }
        }
    }

    void HandleE(bool canPutState)
    {
        if (canPutState)
        {
            inNeedle = !inNeedle;
            TryToPutNeedle();
        }
        else
        {
            // cancel trap
            if (inNeedle)
            {
                Destroy(tempNeedleTrap);
                tempNeedleTrap = null;
                inNeedle = !inNeedle;
            }
        }
    }

    void HandleShift()
    {
        if (canPut)
        {
            int trapType = -1;
            Vector3 trapLocation = new Vector3();
            if (tempIceTrap != null)
            {
                trapType = GameSettings.ICE_TRAP_TYPE;
                trapLocation = tempIceTrap.transform.position;
                Destroy(tempIceTrap);
                tempIceTrap = null;
            }
            else if (tempNeedleTrap != null)
            {
                trapType = GameSettings.NEEDLE_TRAP_TYPE;
                trapLocation = tempNeedleTrap.transform.position;
                Destroy(tempNeedleTrap);
                tempNeedleTrap = null;
            }

            SendPutTrapMsg(trapType, GameSettings.playerID, trapLocation);

            inIce = inNeedle = canPut = false;

        }
    }

    void SendPutTrapMsg(int trapType, int playerID, Vector3 trapLcoation)
    {
        ClientPutTrapMsg clientPutTrapMsg = new ClientPutTrapMsg();
        clientPutTrapMsg.trapType = trapType;
        clientPutTrapMsg.playerID = playerID;
        clientPutTrapMsg.location.SetLocation(trapLcoation);

        string clientPutTrapMsgJson = JsonConvert.SerializeObject(clientPutTrapMsg);

        byte[] msg = MessageHandler.SetClientMsg(
           NetworkSettings.TRAP_ENTITY_SERVICE_ID,
           NetworkSettings.TRAP_ENTITY_CREATE_CMD,
           clientPutTrapMsgJson);

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

    void PutTrap()
    {
        if(tempIceTrap != null)
        {
            baseShaderMaterial = tempIceTrap.GetComponent<MeshRenderer>().materials[0];
            baseShaderMaterial.shader = Shader.Find("Standard");
            baseShaderMaterial.color = new Color(1f, 1f, 1f, 1f);
            tempIceTrap.GetComponent<Animation>().enabled = true;
            BoxCollider newCollider = tempIceTrap.AddComponent<BoxCollider>();
            newCollider.size = new Vector3(0.8f, 0.05f, 0.8f);
            newCollider.center = new Vector3(0, 0, 0);
        }
        else if(tempNeedleTrap != null)
        {
            baseShaderMaterial = null;
            needleShaderMaterial = null;
            foreach (Transform child in tempNeedleTrap.transform)
            {
                if (child.gameObject.name == "Trap_Needle")
                    baseShaderMaterial = child.gameObject.GetComponent<MeshRenderer>().materials[0];
                else if (child.gameObject.name == "Needle")
                    needleShaderMaterial = child.gameObject.GetComponent<MeshRenderer>().materials[0];

            }
            baseShaderMaterial.shader = Shader.Find("Standard");
            needleShaderMaterial.shader = Shader.Find("Standard");
            baseShaderMaterial.color = new Color(1f, 1f, 1f, 1f);
            needleShaderMaterial.color = new Color(1f, 1f, 1f, 1f);
            tempNeedleTrap.GetComponent<Animation>().enabled = true;
            BoxCollider newCollider = tempNeedleTrap.AddComponent<BoxCollider>();
            newCollider.size = new Vector3(0.8f, 0.05f, 0.8f);
            newCollider.center = new Vector3(0, 0, 0);
        }

        inIce = inNeedle = canPut = false;
        tempIceTrap = null;
        tempNeedleTrap = null;
    }

    void MoveTempTrap()
    {
        Vector3 targetPoint = GetTargetPoint();

        if (tempIceTrap != null)
        {
            tempIceTrap.transform.position = targetPoint;
            baseShaderMaterial = tempIceTrap.GetComponent<MeshRenderer>().materials[0];
            baseShaderMaterial.shader = Shader.Find("GUI/Text Shader");
            if (CheckTempTrap(tempIceTrap))
            {
                baseShaderMaterial.color = new Color(0.2f, 0.8f, 0.2f, 0.7f);
                canPut = true;
            }
            else
            {
                baseShaderMaterial.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
                canPut = false;
            }
        }
        else if (tempNeedleTrap != null)
        {
            tempNeedleTrap.transform.position = targetPoint;
            baseShaderMaterial = null;
            needleShaderMaterial = null;
            foreach (Transform child in tempNeedleTrap.transform)
            {
                if (child.gameObject.name == "Trap_Needle")
                    baseShaderMaterial = child.gameObject.GetComponent<MeshRenderer>().materials[0];
                else if(child.gameObject.name == "Needle")
                    needleShaderMaterial = child.gameObject.GetComponent<MeshRenderer>().materials[0];

            }
            baseShaderMaterial.shader = Shader.Find("GUI/Text Shader");
            needleShaderMaterial.shader = Shader.Find("GUI/Text Shader");
            if (CheckTempTrap(tempNeedleTrap))
            {
                baseShaderMaterial.color = new Color(0.2f, 0.8f, 0.2f, 0.7f);
                needleShaderMaterial.color = new Color(0.2f, 0.8f, 0.2f, 0.7f);
                canPut = true;
            }
            else
            {
                baseShaderMaterial.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
                needleShaderMaterial.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
                canPut = false;
            }
        }
    }

    bool CheckTempTrap(GameObject tempTrap)
    {
        if (tempTrap.transform.position.y > GameSettings.ENVIRONMENT_FLOOR_HEIGHT + 0.02f)
            return false;

        Vector3 center = tempTrap.transform.position;
        if(center.y < -7.67)
            center.y += 0.02f;
        Vector3 halfExtents = new Vector3(GameSettings.TRAP_WIDTH / 2, 0.01f, GameSettings.TRAP_WIDTH / 2);
        Collider[] colliders =  Physics.OverlapBox(center, halfExtents, new Quaternion(0, 0, 0, 1f));

        return colliders.Length == 0;
    }

    Vector3 GetTargetPoint()
    {
        RaycastHit shootHit;
        shootRay.origin = shootingPivot.transform.position;
        shootRay.direction = shootingPivot.transform.forward;
        Vector3 targetPoint;
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
            targetPoint = shootHit.point;
        else
            targetPoint = shootRay.origin + shootRay.direction * range;

        targetPoint.x = (float)System.Math.Round(targetPoint.x, 0);
        targetPoint.z = (float)System.Math.Round(targetPoint.z, 0);
        return targetPoint;
    }

    void TryToPutNeedle()
    {
        Vector3 targetPoint = GetTargetPoint();
        tempNeedleTrap = Instantiate(NeedleTrap, targetPoint, new Quaternion(0, 0, 0, 1));
        tempNeedleTrap.GetComponent<Animation>().enabled = false;
        tempNeedleTrap.GetComponent<BoxCollider>().enabled = false;
    }

    void TryToPutIce()
    {
        Vector3 targetPoint = GetTargetPoint();
        tempIceTrap = Instantiate(IceTrap, targetPoint, new Quaternion(0, 0, 0, 1));
        tempIceTrap.GetComponent<Animation>().enabled = false;
        tempIceTrap.GetComponent<BoxCollider>().enabled = false;
    }
}
