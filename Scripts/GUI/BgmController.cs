using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制背景音乐播放状态，并同步按钮上的开关文本和声音图标。
/// </summary>
public class BgmController : MonoBehaviour
{
    public AudioSource audioSource;     // 背景音乐音源，负责循环播放场景中的 BGM 音频
    public TMP_Text toggleButtonText;   // BGM 按钮文字，用于提示当前点击后的操作
    public Image toggleButtonIcon;      // BGM 按钮图标，用于常驻按钮显示当前声音状态
    public Sprite soundOnSprite;        // BGM 开启时显示的声音图标
    public Sprite soundOffSprite;       // BGM 关闭时显示的静音图标
    public bool playOnStart = true;     // 是否在进入播放模式后自动播放 BGM

    private bool bgmEnabled = true;     // 当前 BGM 是否开启，关闭时仅静音不销毁音源

    /// <summary>
    /// 场景加载完成后主动查找 BGM 控制器并启动音乐，避免生命周期顺序导致自动播放遗漏。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoStartSceneBgm()
    {
        BgmController controller = FindObjectOfType<BgmController>();
        if (controller != null)
        {
            controller.ForceStartBgm();
        }
    }
    /// <summary>
    /// 初始化音源循环播放状态，并刷新按钮显示。
    /// </summary>
    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = playOnStart;
            audioSource.mute = !bgmEnabled;

            // 开局界面也需要有背景音乐，先直接尝试启动，若失败由 Update 继续兜底。
            ForceStartBgm();
        }

        UpdateButtonVisual();
    }

    /// <summary>
    /// 音频加载完成后补偿自动播放，避免 Play Mode 刚启动时漏播。
    /// </summary>
    private void Update()
    {
        if (bgmEnabled && audioSource != null && !audioSource.isPlaying)
        {
            ForceStartBgm();
        }
    }

    /// <summary>
    /// 切换 BGM 开关状态，供菜单按钮点击调用。
    /// </summary>
    public void ToggleBgm()
    {
        bgmEnabled = !bgmEnabled;

        if (audioSource != null)
        {
            audioSource.mute = !bgmEnabled;
            if (bgmEnabled && !audioSource.isPlaying)
            {
                ForceStartBgm();
            }
        }

        UpdateButtonVisual();
    }

    /// <summary>
    /// 在音频可播放且 BGM 开启时保持循环播放。
    /// </summary>
    public void ForceStartBgm()
    {
        if (!bgmEnabled || !playOnStart)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null || audioSource.clip == null)
        {
            return;
        }

        audioSource.mute = false;
        audioSource.Play();
    }

    /// <summary>
    /// 根据当前 BGM 状态刷新按钮文字和图标。
    /// </summary>
    private void UpdateButtonVisual()
    {
        if (toggleButtonText != null)
        {
            toggleButtonText.text = bgmEnabled ? "关闭BGM" : "开启BGM";
        }

        if (toggleButtonIcon != null)
        {
            Sprite targetSprite = bgmEnabled ? soundOnSprite : soundOffSprite;
            if (targetSprite != null)
            {
                toggleButtonIcon.sprite = targetSprite;
            }
        }
    }
}
