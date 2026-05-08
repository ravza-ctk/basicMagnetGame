using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public GameObject replayButton;
    public GameObject tasPrefab;

    // 4 BÖLGE VE 4 YAZI EKLENDİ
    public Transform solBolge, sagBolge, ustBolge, altBolge;
    public TextMeshProUGUI oyuncu1Text, oyuncu2Text, oyuncu3Text, oyuncu4Text, sureText, kazananText;

    private int oyuncu1Tas = 10, oyuncu2Tas = 10, oyuncu3Tas = 10, oyuncu4Tas = 10;
    private float kalanSure = 10f;

    // YENİ DÖNGÜSEL SIRA SİSTEMİ (1, 2, 3 veya 4)
    public int siradakiOyuncu = 1;

    private bool oyunDevamEdiyor = true;
    public bool siraDegisiyorMu = false;
    public int sonHamleYapan = 1;

    void Awake() { instance = this; }

    void Start()
    {
        if (kazananText != null) kazananText.gameObject.SetActive(false);
        ArayuzuGuncelle();
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TaslariDiz(solBolge, 10, 1);
            TaslariDiz(sagBolge, 10, 2);
            TaslariDiz(ustBolge, 10, 3);
            TaslariDiz(altBolge, 10, 4);
        }
    }

    void TaslariDiz(Transform bolge, int adet, int sahipNo)
    {
        for (int i = 0; i < adet; i++)
        {
            float xFarki = (i % 2) * 1.5f;
            float zFarki = (i / 2) * 1.5f;
            Vector3 pos = bolge.position + new Vector3(xFarki, 0, zFarki);

            GameObject yeni = PhotonNetwork.Instantiate("tas", pos, Quaternion.identity);
            yeni.GetComponent<PhotonView>().RPC("SetTasSahibi", RpcTarget.AllBuffered, sahipNo);
        }
    }

    void Update()
    {
        if (!oyunDevamEdiyor || siraDegisiyorMu) return;

        kalanSure -= Time.deltaTime;
        sureText.text = "Süre: " + Mathf.Ceil(kalanSure);

        if (kalanSure <= 0)
        {
            sonHamleYapan = siradakiOyuncu; // Süre biterse hamle sırası sonrakine geçer
            SirayiRakibeVer();
        }
    }

    public void HamleYapildi(int oyuncuNo, bool yeniTasMi)
    {
        GetComponent<PhotonView>().RPC("RPC_HamleYapildi", RpcTarget.All, oyuncuNo, yeniTasMi);
    }

    [PunRPC]
    void RPC_HamleYapildi(int oyuncuNo, bool yeniTasMi)
    {
        siraDegisiyorMu = true;
        if (yeniTasMi) { TasSayisiniDegistir(oyuncuNo, -1); }

        CancelInvoke("SirayiRakibeVer");
        Invoke("SirayiRakibeVer", 1f);
    }

    public void SirayiRakibeVer()
    {
        if (!oyunDevamEdiyor) return;

        // SIRAYI BİR SONRAKİ OYUNCUYA GEÇİR
        siradakiOyuncu++;
        if (siradakiOyuncu > 4)
        {
            siradakiOyuncu = 1;
        }

        kalanSure = 10f;
        sureText.text = "Süre: 10";
        siraDegisiyorMu = false;

        // Basit oyun bitiş kontrolü
        if (oyuncu1Tas <= 0 && oyuncu2Tas <= 0 && oyuncu3Tas <= 0 && oyuncu4Tas <= 0) OyunBitti(1);
    }

    public void TasiGeriGonder(GameObject cezaTasi)
    {
        SahaSiniri sinir = cezaTasi.GetComponent<SahaSiniri>();
        if (sinir == null || !sinir.oyundaMi) return;

        CancelInvoke("SirayiRakibeVer");
        siraDegisiyorMu = true;

        int cezaYiyen = sonHamleYapan;

        // HANGİ OYUNCU CEZA YEDİYSE ONUN BÖLGESİNE GÖNDER
        Transform hedef = solBolge;
        if (cezaYiyen == 1) hedef = solBolge;
        else if (cezaYiyen == 2) hedef = sagBolge;
        else if (cezaYiyen == 3) hedef = ustBolge;
        else if (cezaYiyen == 4) hedef = altBolge;

        cezaTasi.GetComponent<TasKontrol>().tasSahibi = cezaYiyen;
        TasSayisiniDegistir(cezaYiyen, 1);

        Vector3 pos = hedef.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        cezaTasi.transform.position = pos;

        Rigidbody rb = cezaTasi.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        sinir.oyundaMi = false;
        cezaTasi.GetComponent<TasKontrol>().sahadaMi = false;

        CancelInvoke("CezaSonrasiSiraDevret");
        Invoke("CezaSonrasiSiraDevret", 0.5f);
    }

    void CezaSonrasiSiraDevret()
    {
        SirayiRakibeVer();
    }

    public void TasSayisiniDegistir(int oyuncuNo, int miktar)
    {
        if (oyuncuNo == 1) oyuncu1Tas += miktar;
        else if (oyuncuNo == 2) oyuncu2Tas += miktar;
        else if (oyuncuNo == 3) oyuncu3Tas += miktar;
        else if (oyuncuNo == 4) oyuncu4Tas += miktar;
        ArayuzuGuncelle();
    }

    void ArayuzuGuncelle()
    {
        if (oyuncu1Text != null) oyuncu1Text.text = "Oyuncu 1: " + oyuncu1Tas;
        if (oyuncu2Text != null) oyuncu2Text.text = "Oyuncu 2: " + oyuncu2Tas;
        if (oyuncu3Text != null) oyuncu3Text.text = "Oyuncu 3: " + oyuncu3Tas;
        if (oyuncu4Text != null) oyuncu4Text.text = "Oyuncu 4: " + oyuncu4Tas;
    }

    void OyunBitti(int kazananNo)
    {
        oyunDevamEdiyor = false;
        if (kazananText != null)
        {
            kazananText.gameObject.SetActive(true);
            kazananText.text = "TEBRİKLER\nBİRİLERİ KAZANDI!";
        }

        if (replayButton != null) replayButton.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}