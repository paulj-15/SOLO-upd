using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using StarterAssets;    

public class TPSHandler : MonoBehaviour
{
    [SerializeField] private Slider healthBar, ammoBar;
    [SerializeField] private Image cooldownTimer;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();

    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private float coolDownTime;
    [SerializeField] private float maxHealthPoints;
    [SerializeField] private float AttackRange;
    [SerializeField] private int usableammosCount = 5;
    [SerializeField] private int maxAmmos = 20;
    [SerializeField] private float hitRate;
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    [SerializeField]
    LayerMask enemyLayer;

    [SerializeField]
    private Transform rayHitPointObject;
    [SerializeField]
    private Transform projectileSpawningPoint;

    [SerializeField]
    private GameObject projectileObject;

    [SerializeField]
    private TwoBoneIKConstraint twoboneConstraints;
    [SerializeField]
    private Rig handRig;
    private int currentUsedAmmo = 0, remainingAmmos;
    private float healthPoints;
    private float timeElapsed;


    private bool doCoolDownTimer = false, doAim = false, enemyInAttackRange;
    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
        ammoBar.maxValue = maxAmmos;
        ammoBar.minValue = 0;
        ammoBar.value = maxAmmos;
        remainingAmmos = maxAmmos - usableammosCount;
/*        if (usableammosCount <= maxAmmos && usableammosCount > 0)
            maxAmmos -= usableammosCount;*/

        healthBar.maxValue = maxHealthPoints;
        healthBar.minValue = 1;
        healthBar.value = maxHealthPoints;
        healthPoints = maxHealthPoints;

        //starterAssetsInputs.shoot = false;

    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        enemyInAttackRange = Physics.CheckSphere(transform.position, AttackRange, enemyLayer);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            rayHitPointObject.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }
        if (currentUsedAmmo >= usableammosCount && !doCoolDownTimer)
        {
            Debug.Log("Time to reload");
            doCoolDownTimer = true;
        }
        if (doCoolDownTimer)
        {
            CoolDownTimer();
        }
        if (starterAssetsInputs.aim || doAim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotationOnMove(false);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            //handRig.weight = Mathf.Lerp(1, 0, Time.deltaTime * 10f);
            //handRig.weight = 1;
            //twoboneConstraints.weight = Mathf.Lerp(1, 0, Time.deltaTime * 10f);

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotationOnMove(true);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            //handRig.weight = Mathf.Lerp(0, 1, Time.deltaTime * 10f);
            //handRig.weight = 0;
            //twoboneConstraints.weight = Mathf.Lerp(0, 1, Time.deltaTime * 10f);
        }

        if ((starterAssetsInputs.shoot || enemyInAttackRange) && (starterAssetsInputs.aim || doAim))
        {
            /*            Vector3 aimDir = (mouseWorldPosition - projectileSpawningPoint.position).normalized;
                        Instantiate(projectileObject, projectileSpawningPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));*/
            //Single shoot
            //camAnim.Play(camAnim.clip.name);
            Debug.Log("trying to shoot");
            Vector3 aimDir = (mouseWorldPosition - projectileSpawningPoint.position).normalized;
            if(remainingAmmos <= maxAmmos && remainingAmmos >= 0)
            {
                if (currentUsedAmmo < usableammosCount)
                {
                    Debug.Log("Ammos used within range");
                    Instantiate(projectileObject, projectileSpawningPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));// projectileSpawningPoint.rotation);
                    currentUsedAmmo += 1;
                    Debug.Log($"current used ammo cnt {currentUsedAmmo} and usable ammo cnt {usableammosCount}");
                    ammoBar.value -= 1;
                }
                else if (currentUsedAmmo >= usableammosCount)
                {
                    Debug.Log("usable ammo count exceeded! wait for reload");


                }
            }
            else
            {
                Debug.Log("Ammo pouch is empty");
            }

            starterAssetsInputs.shoot = false;


        }


    }

    private void OnCollisionEnter(Collision collision)
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"collision object {other.name}");
        if (other.tag == "Attack")
        {
            healthPoints -= hitRate;
            Debug.Log("player attacked");
        }
        if (other.tag == "HP")
        {
            if (healthPoints < maxHealthPoints || healthPoints <= 0)
                healthPoints += 25;
            else
                Debug.Log("Health full");
        }
        if (other.tag == "Ammo")
        {
            var collectible = other.GetComponent<Collectible>();
            if (remainingAmmos < maxAmmos || remainingAmmos < 0)
                remainingAmmos += collectible.CollectibleValue;
            else if(remainingAmmos == maxAmmos)
                Debug.Log("Ammo full");
        }
    }

    private void CoolDownTimer()
    {
        if (!cooldownTimer.gameObject.activeSelf)
        {
            Debug.Log("Reload started");
            cooldownTimer.gameObject.SetActive(true);
        }

        if (timeElapsed < coolDownTime)
        {
            cooldownTimer.fillAmount = Mathf.Lerp(1, 0, timeElapsed / coolDownTime);
            timeElapsed += Time.deltaTime;
        }
        else
        {
            cooldownTimer.fillAmount = 0;
            timeElapsed = 0f;
            cooldownTimer.gameObject.SetActive(false);
            doCoolDownTimer = false;
            currentUsedAmmo = 0;
            remainingAmmos -= usableammosCount;
            Debug.Log("Reload Ended");

        }
    }

    public void SetAim()
    {
        if (!doAim)
            doAim = true;
        else
            doAim = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
