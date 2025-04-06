using System;
using UnityEngine;

public class ParallaxSpriteRenderer : MonoBehaviour
{
    public Transform cameraTransform; // Ссылка на камеру
    public Vector2 parallaxMultiplier; // Множитель параллакса
    public Vector2 tileSize; // Размер тайла (по X и Y)
    
    private Vector3 lastCameraPosition;
    private Vector2 offset;

    private void Start()
    {
        lastCameraPosition = cameraTransform.position;

        // Если у тебя есть конкретный размер тайла, задай его, иначе возьми из текстуры
        if (tileSize == Vector2.zero)
        {
            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            tileSize = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
        }
    }

    private void OnValidate()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        } 
        
        if (tileSize == Vector2.zero)
        {
            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            tileSize = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
        }
    }

    
    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Параллакс-эффект
        transform.position += new Vector3(deltaMovement.x * parallaxMultiplier.x, deltaMovement.y * parallaxMultiplier.y);
        lastCameraPosition = cameraTransform.position;

        // Проверка на выход за границы текстуры и смещение объекта для создания тайлинга
        if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= tileSize.x)
        {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % tileSize.x;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
        }

        if (Mathf.Abs(cameraTransform.position.y - transform.position.y) >= tileSize.y)
        {
            float offsetPositionY = (cameraTransform.position.y - transform.position.y) % tileSize.y;
            transform.position = new Vector3(transform.position.x, cameraTransform.position.y + offsetPositionY);
        }
    }
}