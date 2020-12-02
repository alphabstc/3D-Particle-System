using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class particleData {
	public float radius = 0, angle = 0, time = 0;
	public particleData(float radius, float angle, float time){//构造函数
		this.radius = radius;//粒子半径
		this.angle = angle;//粒子角度
		this.time = time;//时间，影响粒子漂浮时的大小
	}
}
public class ParticleHalo : MonoBehaviour {
	private ParticleSystem parSys;//对应粒子系统
	private ParticleSystem.Particle[] particleArr; //存储粒子
	private particleData[] particles;//存储对应粒子的参数
	private float[] radius; //外圈粒子半径
	private float[] collect_radius;//内圈粒子半径
	private int tier = 15;//层数
	private int time = 0;//时间
	public Gradient colorGradient;//颜色梯度
	public int particleNum = 30000;//粒子数目
	public float size = 0.03f;//粒子大小
	public float minRadius = 6.0f;//外圈最小半径
	public float maxRadius = 12.0f;//外圈最大半径
	public float collect_MaxRadius = 4.0f;//内圈最大半径
	public float collect_MinRadius = 1.0f;//内圈最小半径
	public bool clockwise = true; //是否顺时针旋转
	public float speed = 1.5f; //速度
	public float pingPong = 0.02f; //粒子处理间隔
	public int isCollected = 0;//确定当前是内圈还是/外圈
	void Start(){//初始化
		particleArr = new ParticleSystem.Particle[particleNum];//创建粒子数组
		particles = new particleData[particleNum];//创建粒子参数数组
		radius = new float[particleNum];//创建浮点数数组
		collect_radius = new float[particleNum];//创建浮点数数组
		parSys = this.GetComponent<ParticleSystem>();//获得对应粒子系统
		var main = parSys.main;//获得对应的主参数
		main.startSpeed = 0;  //设置初始速度
		main.startSize = size; //设置初始大小
		main.loop = false; //设置是否循环
		main.maxParticles = particleNum; //设置最大粒子数目     
		parSys.Emit(particleNum); //发射粒子
		parSys.GetParticles(particleArr);//获得粒子数组
		RandomlySpread();//随机散播粒子
	}
	void changeColor(){
		float colorValue;
		for (int i = 0; i < particleNum; i++){
			colorValue = (Time.realtimeSinceStartup - Mathf.Floor(Time.realtimeSinceStartup));
			colorValue += particles[i].angle/360;
			while (colorValue > 1) colorValue--;
			particleArr[i].startColor = colorGradient.Evaluate(colorValue);
		}
	}
	void RandomlySpread(){
		for (int i = 0; i < particleNum; ++i){  //对于每个粒子
			float midRadius = (maxRadius + minRadius) / 2;//计算平均半径
			float minRate = UnityEngine.Random.Range(1.0f, midRadius / minRadius);//在1到midRadius / minRadius之间产生一个随机数
			float maxRate = UnityEngine.Random.Range(midRadius / maxRadius, 1.0f);//在midRadius / maxRadius到1之间产生一个随机数
			float _radius = UnityEngine.Random.Range(minRadius * minRate, maxRadius * maxRate);//在minRadius * minRate到maxRadius * maxRate之间产生一个随机数 设置为当前半径
			radius[i] = _radius;//设置半径
			float collect_MidRadius = (collect_MaxRadius + collect_MinRadius) / 2;//计算平均半径
			float collect_outRate = Random.Range(1.0f, collect_MidRadius / collect_MinRadius);;//在1到midRadius / minRadius之间产生一个随机数
			float collect_inRate = Random.Range(collect_MaxRadius / collect_MidRadius, 1f);//在midRadius / maxRadius到1之间产生一个随机数
			float _collect_radius = Random.Range(collect_MinRadius * collect_outRate, collect_MaxRadius * collect_inRate);//在minRadius * minRate到maxRadius * maxRate之间产生一个随机数 设置为当前半径
			collect_radius[i] = _collect_radius;//设置半径
			float angle = UnityEngine.Random.Range(0.0f, 360.0f);//随机获取角度
			float theta = angle / 180 * Mathf.PI;//计算角度出弧度制下对应的值
			float time = UnityEngine.Random.Range(0.0f, 360.0f);//随机获取时间
			if (isCollected == 0) //当前为外圈
				particles[i] = new particleData(_radius, angle, time);//设置为外圈参数
			else//当前为内圈
				particles[i] = new particleData(_collect_radius, angle, time);//设置为内圈参数
			particleArr[i].position = new Vector3(particles[i].radius * Mathf.Cos(theta), 0f, particles[i].radius * Mathf.Sin(theta));//根据半径和角度计算出所处位置
		}
		parSys.SetParticles(particleArr, particleArr.Length);//设定粒子数组
	}
	// Update is called once per frame
	void Update (){
		for (int i = 0; i < particleNum; i++) {
			if (clockwise) particles[i].angle -= (i % tier + 1) * (speed / particles[i].radius / tier);
			else particles[i].angle += (i % tier + 1) * (speed / particles[i].radius / tier);
			particles[i].angle = (360.0f + particles[i].angle) % 360.0f;
			float theta = particles[i].angle / 180 * Mathf.PI;
			if (isCollected == 1){
				if (particles[i].radius > collect_radius [i]) particles[i].radius -= 15f * (collect_radius [i] / collect_radius [i]) * Time.deltaTime;  
			 	else particles[i].radius = collect_radius [i];
			} 
			else {
				if (particles[i].radius < radius [i]) particles[i].radius += 15f * (collect_radius [i] / collect_radius [i]) * Time.deltaTime;  
				else particles[i].radius += Mathf.PingPong (particles[i].time / minRadius / maxRadius, pingPong) - pingPong / 2.0f;
			}
			particleArr [i].position = new Vector3 (particles[i].radius * Mathf.Cos (theta), 0f, particles[i].radius * Mathf.Sin (theta));
		}
		changeColor ();
		parSys.SetParticles(particleArr, particleArr.Length);
	}
	void OnGUI(){
		if(Input.GetMouseButtonDown(0)){
			time++;
			if(time==2){
				isCollected = 1 - isCollected;
				time = 0;
			}
		}
	}
}