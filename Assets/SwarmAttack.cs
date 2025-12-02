using UnityEngine;

public class SwarmAttack : MonoBehaviour {
    public Transform target; // 攻击目标（拖拽赋值）
    public float attractForce = 5f; // 吸引力强度
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;

    void Start() {
        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    void Update() {
        if (target == null) return;

        int count = particleSystem.GetParticles(particles);
        for (int i = 0; i < count; i++) {
            // 计算粒子到目标的方向
            Vector3 dir = (target.position - particles[i].position).normalized;
            // 施加吸引力（叠加到原有速度上）
            particles[i].velocity += dir * attractForce * Time.deltaTime;
        }
        particleSystem.SetParticles(particles, count);
    }
}