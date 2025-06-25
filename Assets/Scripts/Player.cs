using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject _spawnObject;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private string _enemyTag;
    [SerializeField] private string _directionStr;

    private Vector2 _direction = Vector2.down;
    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_directionStr == "up") _direction = Vector2.up;
        else _direction = Vector2.down;
        StartCoroutine(CreateObject());
    }

    void Update()
    {
        // Muda a direção apenas quando uma tecla for pressionada
        if (_directionStr != "up")
        {
            if (Input.GetKeyDown(KeyCode.W) && _direction != Vector2.down)
            {
                _direction = Vector2.up;
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else if (Input.GetKeyDown(KeyCode.S) && _direction != Vector2.up)
            {
                _direction = Vector2.down;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.A) && _direction != Vector2.right)
            {
                _direction = Vector2.left;
                transform.rotation = Quaternion.Euler(0, 0, 270);
            }
            else if (Input.GetKeyDown(KeyCode.D) && _direction != Vector2.left)
            {
                _direction = Vector2.right;
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && _direction != Vector2.down)
            {
                _direction = Vector2.up;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && _direction != Vector2.up)
            {
                _direction = Vector2.down;
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && _direction != Vector2.right)
            {
                _direction = Vector2.left;
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && _direction != Vector2.left)
            {
                _direction = Vector2.right;
                transform.rotation = Quaternion.Euler(0, 0, 270);
            }
        }
    }

    void FixedUpdate()
    {
        // Movimento contínuo, como uma cobrinha
        _rb.velocity = _direction * _speed;
    }

    IEnumerator CreateObject()
    {

        Instantiate(_spawnObject, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(CreateObject());
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(gameObject.tag))
        {
            collision.gameObject.tag = _enemyTag;
            // Debug.Log("Morreu Proprio corpo");
            // Destroy(gameObject);
        }       
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(_enemyTag) || collision.gameObject.CompareTag(gameObject.tag))
        {
            Debug.Log("Morreu Para Inimigo");
            // Destroy(gameObject);
            SceneManager.LoadScene("Game");
        }
    }
}
