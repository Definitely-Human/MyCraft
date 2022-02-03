using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    public float walkSpeed =3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float playerWidth = 0.15f;

    private Transform cam;
    private World world;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.05f;
    public float reach = 8f;


    public ToolBar toolBar;
    public bool Front
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }
    }

    public bool Back
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }
    }

    public bool Left
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z )) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }

    public bool Right
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z )) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z ))
                )
                return true;
            else
                return false;
        }
    }


    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        world.InUI = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            world.InUI = !world.InUI;
        }
        if (!world.InUI)
        {
            GetPlayerInputs();
            placeCursorBlock();
        }        
    }

    private void placeCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);
            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!world.InUI)
        {
            CalculateVelocity();
            if (jumpRequest)
                Jump();

            transform.Rotate(Vector3.up * mouseHorizontal * VoxelData.settings.mouseSensitivity);
            cam.Rotate(Vector3.right * -mouseVertical * VoxelData.settings.mouseSensitivity);
            transform.Translate(velocity, Space.World);
        }
    }

    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        float speed = 0;
        // Affect vertical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // If sprinting use sprint multiplier
        if (isSprinting)
            speed = sprintSpeed;
        else
            speed = walkSpeed;

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * speed;

        //Apply vertical momentum 
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        // Check if movement collides with the terrain
        if ((velocity.z > 0) && Front || (velocity.z < 0) && Back)
            velocity.z = 0;
        if ((velocity.x > 0) && Right || (velocity.x < 0) && Left)
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);
    }

    private void GetPlayerInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if(isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
        

        if (highlightBlock.gameObject.activeSelf )
        {
            // Destroy Block
            if (Input.GetMouseButtonDown(0))
                if( world.GetChunkFromVector3(highlightBlock.position) != null && world.GetChunkFromVector3(highlightBlock.position).IsEditable)
                    world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);

            // Place Block
            if (Input.GetMouseButtonDown(1))
            {
                if (toolBar.slots[toolBar.slotIndex].HasItem && world.GetChunkFromVector3(placeBlock.position) != null && world.GetChunkFromVector3(placeBlock.position).IsEditable)
                {

                    world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolBar.slots[toolBar.slotIndex].itemSlot.stack.Id);
                    toolBar.slots[toolBar.slotIndex].itemSlot.Take(1);
                }
            }
        }
    }

    private float checkDownSpeed(float downSpeed)
    {
        if(
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) 
            )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float checkUpSpeed(float upSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
            )
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }
}
