using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private Queue<T> pool = new Queue<T>();
        private T prefab;
        private Transform parent;
        private IObjectResolver container;
        
        public ObjectPool(T prefab, Transform parent, int initialSize = 10, IObjectResolver container = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.container = container;
            
            // Попередньо створюємо об'єкти
            for (int i = 0; i < initialSize; i++)
            {
                T obj = CreateNewItem();
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }
        
        public T Get()
        {
            T obj;
            
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                obj = CreateNewItem();
            }
            
            obj.gameObject.SetActive(true);
            return obj;
        }
        
        public void Return(T item)
        {
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }
        
        private T CreateNewItem()
        {
            if (container != null)
            {
                // Використовуємо VContainer для створення об'єкта
                GameObject instance = Object.Instantiate(prefab.gameObject, parent);
                container.InjectGameObject(instance);
                return instance.GetComponent<T>();
            }
            else
            {
                // Звичайне створення об'єкта
                return Object.Instantiate(prefab, parent);
            }
        }
    }
} 