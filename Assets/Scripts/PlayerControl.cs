using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
    public float speed = 10f;
    public float jumpPower = 5f;
    Rigidbody rigdbody;
    

    Vector3 movement;
    float horizontalMove;
    float verticalMove;
    float rotatespeed = 1f;
    bool isjumpping;
    

	// Use this for initialization
	void Start () {
	
	}

    void Awake()
    {
        rigdbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Jump"))
            isjumpping = true;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Run();
        Jump();
        Turn();
	}

    /// <summary>
    /// 걸음걸이 함수
    /// </summary>
    void Run()
    {
        movement.Set(horizontalMove, 0, verticalMove);
        movement = movement.normalized * speed * Time.deltaTime;
        rigdbody.MovePosition(transform.position + movement);
    }

    /// <summary>
    /// 점프함수
    /// </summary>
    void Jump()
    {
        if (!isjumpping)
            return;

        rigdbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        isjumpping = false;
    }

    void Turn()
    {
        if (horizontalMove == 0 && verticalMove == 0)
            return;

        Quaternion newRotation = Quaternion.LookRotation(movement);
        rigdbody.rotation = Quaternion.Slerp(rigdbody.rotation, newRotation, rotatespeed*Time.deltaTime);

    }

}
