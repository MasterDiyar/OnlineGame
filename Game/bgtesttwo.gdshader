shader_type canvas_item;

// Параметры, которые можно настраивать из редактора
uniform vec4 color1 : source_color = vec4(0.2, 0.4, 0.8, 1.0);
uniform vec4 color2 : source_color = vec4(0.8, 0.2, 0.4, 1.0);
uniform float speed : hint_range(0.1, 2.0) = 0.5;
uniform float wave_frequency : hint_range(0.1, 10.0) = 3.0;
uniform float wave_amplitude : hint_range(0.0, 0.5) = 0.1;
uniform float noise_scale : hint_range(0.1, 10.0) = 2.0;
uniform float color_mix_speed : hint_range(0.1, 2.0) = 0.3;

// Шум для создания органичных волн
float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

float noise(vec2 p) {
    vec2 ip = floor(p);
    vec2 u = fract(p);
    u = u * u * (3.0 - 2.0 * u);
    
    float res = mix(
        mix(rand(ip), rand(ip + vec2(1.0, 0.0)), u.x),
        mix(rand(ip + vec2(0.0, 1.0)), rand(ip + vec2(1.0, 1.0)), u.x), 
    u.y);
    return res * res;
}

void fragment() {
    // Нормализованные координаты (0-1)
    vec2 uv = UV;
    
    // Добавляем время для анимации
    float time = TIME * speed;
    
    // Создаем волновой эффект
    float wave = sin(uv.x * wave_frequency + time) * wave_amplitude;
    wave += sin(uv.x * wave_frequency * 2.0 + time * 1.5) * wave_amplitude * 0.5;
    
    // Добавляем шум для более органичного вида
    float n = noise(uv * noise_scale + vec2(time * 0.3, time * 0.2));
    wave += n * 0.1;
    
    // Смещаем UV координаты волной
    uv.y += wave;
    
    // Создаем градиент с движением
    float gradient = uv.y + sin(time * color_mix_speed) * 0.2;
    gradient = clamp(gradient, 0.0, 1.0);
    
    // Смешиваем цвета
    vec3 color = mix(color1.rgb, color2.rgb, gradient);
    
    // Добавляем небольшую вариативность
    color += vec3(n * 0.1);
    
    // Устанавливаем цвет пикселя
    COLOR = vec4(color, 1.0);
}