shader_type canvas_item;

uniform float overall_amount : hint_range(0.0, 1.0) = 0.0;

uniform float block_size = 30;

uniform float shake_amount = 1;

uniform float shift_speed = 5.0;
uniform float shift_amount = 12.0;

uniform float shuffle_speed = 3.7;

uniform float rand_offset = 0.0;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

float random(float seed) {
  return fract(sin(dot(vec2(rand_offset + seed, rand_offset + seed), vec2(12.9898, 4.1414))) * 43758.5453);
}

void fragment() {
  vec2 uv = SCREEN_UV;

  vec2 uv_block_size = block_size * SCREEN_PIXEL_SIZE;
  float should_shift = random(trunc(TIME * shift_speed)) * random(trunc(uv.y / uv_block_size.y));

  vec2 displacement_x = SCREEN_PIXEL_SIZE.x * overall_amount * (2.0 * should_shift - 1.0) * shift_amount * vec2(1, 0);
  vec2 displacement_y = SCREEN_PIXEL_SIZE.y * overall_amount * (should_shift * random(trunc(uv.x * block_size)) - 0.5) * shift_amount * vec2(0, 1);

  vec4 color = texture(screen_texture, SCREEN_UV);
  color.r = mix(color.r, texture(screen_texture, SCREEN_UV + displacement_x + displacement_y).r, overall_amount);
  color.b = mix(color.b, texture(screen_texture, SCREEN_UV - displacement_x + displacement_y).b, overall_amount);

  COLOR = color;
}
