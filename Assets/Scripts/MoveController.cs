// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// public class MoveController : MonoBehaviour
// {
//     [SerializeField] private float speed;
//     private float distance;
//     private Vector2 move;
//     private Vector3 targetPos;
//     // [SerializeField] MainManager mainManagerScript;

//     private void Start()
//     {
//         distance = 1.0f;
//         targetPos = transform.position;
//     }
//     void Update()
//     {
//         move.x = Input.GetAxisRaw("Horizontal");
//         move.y = Input.GetAxisRaw("Vertical");
// //        if (move != Vector2.zero && Vector3.Distance(transform.position, targetPos) < 0.5f)
//         if (move != Vector2.zero && transform.position == targetPos)
//         {
//             targetPos += new Vector3(move.x * distance, move.y * distance, 0);
//         }
//         Move(targetPos);
//     }
//     private void Move(Vector3 targetPosition)
//     {
//         transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
//     }
// }