using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject _spawnObject;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _speed;
    private Rigidbody2D _rb;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        StartCoroutine(CreateObject());
    }

    // Update is called once per frame
    void Update()
    {
        _rb.velocity = Vector2.down * _speed * Time.deltaTime;
        // if (Input.GetKeyDown(KeyCode.UpArrow)) transform.rotation += new Vector3(0f,0f,0f);
    }
    IEnumerator CreateObject()
    {
        yield return new WaitForSeconds(1f);
        GameObject newObject = Instantiate(_spawnObject, _spawnPoint.transform.position, Quaternion.identity);
        StartCoroutine(CreateObject());
    }
}
