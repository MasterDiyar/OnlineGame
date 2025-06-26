shader_type canvas_item;

// Настройки
uniform int lines_count = 10;
uniform int triangles_count = 20;        // Количество треугольников
uniform vec4 triangle_color : source_color = vec4(0.2, 0.6, 1.0, 1.0);
uniform vec4 background_color : source_color = vec4(0.05, 0.05, 0.1, 1.0);
uniform vec4 stripe_color : source_color = vec4(1.0, 0.8, 0.3, 1.0);
uniform float min_size = 0.05;          // Минимальный размер
uniform float max_size = 0.2;           // Максимальный размер
uniform float pulse_speed = 0.8;        // Скорость анимации
uniform float seed = 0.0;               // Случайное зерно
uniform float rotation_speed = 0.3;     // Скорость вращения
uniform float stripe_width = 0.02;
uniform float stripe_speed = 0.2;
uniform float stripe_angle = 0.5;

// Функция псевдослучайных чисел
float rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

// Функция определения точки в треугольнике
float triangleDistance(vec2 p, vec2 p0, vec2 p1, vec2 p2) {
    vec2 e0 = p1 - p0;
    vec2 e1 = p2 - p1;
    vec2 e2 = p0 - p2;

    vec2 v0 = p - p0;
    vec2 v1 = p - p1;
    vec2 v2 = p - p2;

    vec2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    vec2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    vec2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);

    float d = min(min(
        dot(pq0, pq0),
        dot(pq1, pq1)),
        dot(pq2, pq2));

    float s = sign(e0.x * e2.y - e0.y * e2.x);
    vec2 d0 = vec2(dot(v0, vec2(-e0.y, e0.x)) * s,
                  dot(v1, vec2(-e1.y, e1.x)) * s);
    vec2 d1 = vec2(dot(v2, vec2(-e2.y, e2.x)) * s,
                  dot(v0, vec2(-e0.y, e0.x)) * s);

    d = sqrt(d) * ((d0.x < 0.0 && d0.y < 0.0 && d1.x < 0.0) ? -1.0 : 1.0);
    return d;
}

void fragment() {
    COLOR = background_color;

	vec2 offset = vec2(float(stripe_angle) * 0.127, float(stripe_angle) * 0.819) + vec2(seed);
	float angle = rand(offset) * 6.283185; // Конвертируем в радианы
    vec2 dir = vec2(cos(angle), sin(angle));
    float stripe_pattern = fract(dot(UV, dir) * float(lines_count) + TIME * stripe_speed);
    float stripe = smoothstep(stripe_width*0.4, stripe_width*1.5, abs(stripe_pattern-0.5));
    COLOR = mix(COLOR, stripe_color, stripe+1.);

    for (int i = 0; i < triangles_count; i++) {
        // Генерируем уникальные параметры для каждого треугольника
        vec2 offset = vec2(float(i) * 0.357, float(i) * 0.159) + vec2(seed);
        vec2 center = vec2(rand(offset), rand(offset + vec2(1.0)));

        // Анимируем размер и вращение
        float phase = rand(offset + vec2(2.0)) * 6.283;
        float pulse = sin(TIME * pulse_speed + phase) * 0.5 + 0.5;
        float size = mix(min_size, max_size, pulse);
        float angle = TIME * rotation_speed * (rand(offset + vec2(3.0)) * 2.0 - 1.0);

        // Вершины равностороннего треугольника
        vec2 p0 = center + size * vec2(0.0, -0.577);
        vec2 p1 = center + size * vec2(-0.5, 0.289);
        vec2 p2 = center + size * vec2(0.5, 0.289);

        // Вращаем вершины
        mat2 rot = mat2(vec2(cos(angle), -sin(angle)),
                     vec2(sin(angle),  cos(angle)));
        p0 = center + rot * (p0 - center);
        p1 = center + rot * (p1 - center);
        p2 = center + rot * (p2 - center);

    float dist = triangleDistance(UV, p0, p1, p2);
    float edge_width = 0.005;
    float triangle_factor = smoothstep(edge_width, -edge_width, dist);

    // Добавляем свечение
    float glow = 1.0 - smoothstep(0.0, edge_width * 3.0, abs(dist));
    vec4 glow_color = mix(triangle_color, vec4(1.0), 0.7);

    COLOR = mix(COLOR, glow_color, glow * 0.3);
    COLOR = mix(COLOR, triangle_color, triangle_factor);
    }

}
