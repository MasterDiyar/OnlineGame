shader_type canvas_item;

uniform float A = 1.0;
uniform bool polar_coordinates = false;
uniform vec2 polar_center = vec2(0.5);
uniform float polar_zoom = 1.0;
uniform float polar_repeat = 1.0;

uniform float spin_rotation;
uniform float spin_speed = 1.0;
uniform vec2 offset = vec2(0.0, 0.0);
uniform float contrast = 2.0;
uniform float spin_amount = 0.36;
uniform float pixel_filter = 700.0;

#define SPIN_EASE 1.0

// --- Полярные координаты ---
vec2 polar_coords(vec2 uv, vec2 center, float zoom, float repeat){
	vec2 dir = uv - center;
	float radius = length(dir) * 2.0;
	float angle = atan(dir.y, dir.x) / (PI * 2.0);
	return mod(vec2(radius * zoom, angle * repeat), 1.0);
}

// --- Деформирующий эффект ---
vec2 distorted_uv(vec2 screenSize, vec2 screen_coords){
	float pixel_size = length(screenSize) / pixel_filter;
	vec2 uv = (floor(screen_coords * (1.0 / pixel_size)) * pixel_size - 0.5 * screenSize) / length(screenSize) - offset;
	float uv_len = length(uv);

	float speed = (spin_rotation * SPIN_EASE * 0.2) + 302.2;
	float new_pixel_angle = atan(uv.y, uv.x) + speed - SPIN_EASE * 20.0 * (spin_amount * uv_len + (1.0 - spin_amount));
	vec2 mid = screenSize / length(screenSize) / 2.0;
	uv = vec2(uv_len * cos(new_pixel_angle) + mid.x, uv_len * sin(new_pixel_angle) + mid.y) - mid;

	uv *= 30.0;
	speed = TIME * spin_speed;
	vec2 uv2 = vec2(uv.x + uv.y);

	for (int i = 0; i < 5; i++) {
		uv2 += sin(max(uv.x, uv.y)) + uv;
		uv += 0.5 * vec2(cos(5.1123314 + 0.353 * uv2.y + speed * 0.131121), sin(uv2.x - 0.113 * speed));
		uv -= 1.0 * cos(uv.x + uv.y) - 1.0 * sin(uv.x * 0.711 - uv.y);
	}

	// Преобразуем обратно в диапазон UV
	vec2 final_uv = uv * 0.035 * contrast;
	return fract(final_uv + 0.5); // Без выхода за пределы текстуры
}

void fragment() {
	vec2 screenSize = vec2(1.0, 1.0);
	vec2 coords = UV;

	if (polar_coordinates){
		coords = polar_coords(UV, polar_center, polar_zoom, polar_repeat);
	}

	vec2 new_uv = distorted_uv(screenSize, coords);
	COLOR = texture(TEXTURE, new_uv) * COLOR;
	COLOR.a = A;
}
