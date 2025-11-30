using UnityEngine;


public class PlyerMovement : MonoBehaviour
{

    public float speed = 5f;
    public float JumpForce = 2f;
    public int JumpCount = 2;
    private int Jump = 0;
    private int Coin = 0;


    private Rigidbody rb;
    private Collider tempCoin;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);



        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Jump < JumpCount)
            {
                rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                Jump++;
            }
        }


        // Rotate player to face movement direction
        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(movement.x, 0f, movement.z));
        }


    }



    private void OnCollisionEnter(Collision GR)
    {
        if (GR.gameObject.CompareTag("Ground"))
        {
            Jump = 0;

        }
    }




    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Coins") && tempCoin != other)
        {
            tempCoin = other;
        
            print(other.gameObject.name);
            AudioSource coins = other.GetComponent<AudioSource>();
            coins.Play();
            other.gameObject.GetComponent<MeshFilter>().mesh = null;

            Destroy(other.gameObject, 1f);
        }
    }
}
