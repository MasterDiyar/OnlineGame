shader_type canvas_item;

// === ПАРАМЕТРЫ ===
uniform float A = 1.0;
uniform bool polar_coordinates = false;
uniform vec2 polar_center = vec2(0.5);
uniform float polar_zoom = 1.0;
uniform float polar_repeat = 1.0;

uniform float spin_rotation;
uniform float spin_speed = 1.0;
uniform vec2 offset = vec2(0.0, 0.0);
uniform vec4 colour_1 : source_color;
uniform vec4 colour_2 : source_color;
uniform vec4 colour_3 : source_color;
uniform float contrast = 2.0;
uniform float spin_amount = 0.36;
uniform float pixel_filter = 700.0;

#define SPIN_EASE 1.0

// === ПЕРЕВОД В ПОЛЯРНЫЕ КООРДИНАТЫ ===
vec2 polar_coords(vec2 uv, vec2 center, float zoom, float repeat){
	vec2 dir = uv - center;
	float radius = length(dir) * 2.0;
	float angle = atan(dir.y, dir.x) / (PI * 2.0);
	return mod(vec2(radius * zoom, angle * repeat), 1.0);
}

// === ОСНОВНОЙ ЭФФЕКТ ===
vec4 effect(vec2 screenSize, vec2 screen_coords){
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

	float contrast_mod = (0.25 * contrast + 0.5 * spin_amount + 1.2);
	float paint_res = min(2.0, max(0.0, length(uv) * 0.035 * contrast_mod));
	float c1p = max(0.0, 1.0 - contrast_mod * abs(1.0 - paint_res));
	float c2p = max(0.0, 1.0 - contrast_mod * abs(paint_res));
	float c3p = 1.0 - min(1.0, c1p + c2p);

	vec4 ret_col = (0.3 / contrast) * colour_1 + (1.0 - 0.3 / contrast) * (colour_1 * c1p + colour_2 * c2p + vec4(c3p * colour_3.rgb, c3p * colour_1.a));
	return ret_col;
}

// === fragment ===
void fragment() {
	vec2 screenSize = vec2(1.0, 1.0); // Эмуляция screenSize для Polygon2D без текстуры
	vec2 coords = UV;

	if (polar_coordinates) {
		coords = polar_coords(UV, polar_center, polar_zoom, polar_repeat);
	}
	
	COLOR = effect(screenSize, coords);
	COLOR.a = A;
}
