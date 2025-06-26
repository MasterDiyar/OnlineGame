shader_type canvas_item;

// Настройки
uniform int squares_count = 16;          // Количество квадратов
uniform vec4 square_color : source_color = vec4(0.8, 0.3, 0.2, 1.0);
uniform vec4 background_color : source_color = vec4(0.1, 0.1, 0.1, 1.0);
uniform float min_size = 0.05;           // Минимальный размер
uniform float max_size = 0.15;           // Максимальный размер
uniform float pulse_speed = 1.0;         // Скорость анимации
uniform float seed = 0.0;                // Случайное зерно

// Функция псевдослучайных чисел
float rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
    COLOR = background_color;
    
    for (int i = 0; i < squares_count; i++) {
        // Генерируем уникальные параметры для каждого квадрата
        vec2 offset = vec2(float(i) * 0.123, float(i) * 0.456) + vec2(seed);
        vec2 square_pos = vec2(rand(offset), rand(offset + vec2(1.0)));
        
        // Анимируем размер с разной фазой для каждого квадрата
        float phase = rand(offset + vec2(2.0)) * 6.283; // 0-2π
        float pulse = sin(TIME * pulse_speed + phase) * 0.5 + 0.5; // 0-1
        float size = mix(min_size, max_size, pulse);
        
        // Определяем границы квадрата
        vec2 square_uv = abs(UV - square_pos) / size;
        float is_square = step(max(square_uv.x, square_uv.y), 0.5);
        
        // Смешиваем цвета
        COLOR = mix(COLOR, square_color, is_square);
    }
}
