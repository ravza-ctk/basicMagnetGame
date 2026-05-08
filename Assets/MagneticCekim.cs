using UnityEngine;

public class MagnetikCekim : MonoBehaviour
{
    public float cekimGucu = 150f; 
    public float etkiAlani = 2f;  

    void FixedUpdate()
    {
        
        Collider[] yakindakiObjeler = Physics.OverlapSphere(transform.position, etkiAlani);

        foreach (Collider obje in yakindakiObjeler)
        {
            if (obje.gameObject != gameObject && obje.gameObject.name.Contains("tas"))
            {
                Rigidbody digerTaþýnFiziði = obje.GetComponent<Rigidbody>();

                if (digerTaþýnFiziði != null)
                {
                    Vector3 cekimYonu = (transform.position - obje.transform.position).normalized;

                    digerTaþýnFiziði.AddForce(cekimYonu * cekimGucu * Time.fixedDeltaTime, ForceMode.Force);
                }
            }
        }
    }
}