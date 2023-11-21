use crate::*;
use interoptopus::{ffi_type, patterns::option::FFIOption};
use rosu_pp::{
    beatmap::BeatmapAttributes, catch::CatchPerformanceAttributes,
    mania::ManiaPerformanceAttributes, osu::OsuPerformanceAttributes,
    taiko::TaikoPerformanceAttributes, Beatmap, GameMode, PerformanceAttributes,
};

#[ffi_type]
#[repr(C)]
#[derive(Clone, Default, PartialEq)]
#[allow(non_snake_case)]
pub struct CalculateResult {
    pub mode: Mode,
    pub stars: f64,
    pub pp: f64,
    pub ppAcc: FFIOption<f64>,
    pub ppAim: FFIOption<f64>,
    pub ppFlashlight: FFIOption<f64>,
    pub ppSpeed: FFIOption<f64>,
    pub ppStrain: FFIOption<f64>,
    pub ppDifficulty: FFIOption<f64>,
    pub nFruits: FFIOption<u32>,
    pub nDroplets: FFIOption<u32>,
    pub nTinyDroplets: FFIOption<u32>,
    pub aimStrain: FFIOption<f64>,
    pub speedStrain: FFIOption<f64>,
    pub flashlightRating: FFIOption<f64>,
    pub sliderFactor: FFIOption<f64>,

    pub ar: f64,
    pub cs: f64,
    pub hp: f64,
    pub od: f64,
    pub bpm: f64,
    pub clockRate: f64,
    pub timePreempt: FFIOption<f64>,
    pub greatHitWindow: FFIOption<f64>,
    pub nCircles: FFIOption<u32>,
    pub nSliders: FFIOption<u32>,
    pub nSpinners: FFIOption<u32>,
    pub maxCombo: FFIOption<u32>,
    pub EffectiveMissCount: FFIOption<f64>,
}

impl CalculateResult {
    pub fn new(
        attrs: PerformanceAttributes,
        map: &Beatmap,
        mods: u32,
        clock_rate: Option<f64>,
    ) -> Self {
        let mut attr_builder = map.attributes();

        if let Some(clock_rate) = clock_rate {
            attr_builder.clock_rate(clock_rate);
        }

        let mode = match &attrs {
            PerformanceAttributes::Catch(_) => GameMode::Catch,
            PerformanceAttributes::Mania(_) => GameMode::Mania,
            PerformanceAttributes::Osu(_) => GameMode::Osu,
            PerformanceAttributes::Taiko(_) => GameMode::Taiko,
        };

        attr_builder.converted(map.mode != mode);

        let BeatmapAttributes {
            ar,
            cs,
            hp,
            od,
            clock_rate,
            hit_windows,
        } = attr_builder.mods(mods).mode(mode).build();

        let bpm = map.bpm() * clock_rate;

        match attrs {
            PerformanceAttributes::Catch(CatchPerformanceAttributes { pp, difficulty }) => Self {
                mode: Mode::Catch,
                pp,
                stars: difficulty.stars,
                maxCombo: Some(difficulty.max_combo() as u32).into(),
                nFruits: Some(difficulty.n_fruits as u32).into(),
                nDroplets: Some(difficulty.n_droplets as u32).into(),
                nTinyDroplets: Some(difficulty.n_tiny_droplets as u32).into(),
                nSpinners: Some(map.n_spinners).into(),
                ar,
                cs,
                hp,
                od,
                bpm,
                clockRate: clock_rate,
                ..Default::default()
            },
            PerformanceAttributes::Mania(ManiaPerformanceAttributes {
                pp,
                pp_difficulty,
                difficulty,
            }) => Self {
                mode: Mode::Mania,
                pp,
                ppDifficulty: Some(pp_difficulty).into(),
                stars: difficulty.stars,
                maxCombo: Some(difficulty.max_combo as u32).into(),
                nCircles: Some(map.n_circles).into(),
                nSliders: Some(map.n_sliders).into(),
                ar,
                cs,
                hp,
                od,
                bpm,
                clockRate: clock_rate,
                greatHitWindow: Some(hit_windows.od).into(),
                ..Default::default()
            },
            PerformanceAttributes::Osu(OsuPerformanceAttributes {
                pp,
                pp_acc,
                pp_aim,
                pp_flashlight,
                pp_speed,
                difficulty,
                effective_miss_count,
            }) => Self {
                mode: Mode::Osu,
                pp,
                ppAcc: Some(pp_acc).into(),
                ppAim: Some(pp_aim).into(),
                ppFlashlight: Some(pp_flashlight).into(),
                ppSpeed: Some(pp_speed).into(),
                stars: difficulty.stars,
                maxCombo: Some(difficulty.max_combo as u32).into(),
                aimStrain: Some(difficulty.aim).into(),
                speedStrain: Some(difficulty.speed).into(),
                flashlightRating: Some(difficulty.flashlight).into(),
                sliderFactor: Some(difficulty.slider_factor).into(),
                nCircles: Some(difficulty.n_circles as u32).into(),
                nSliders: Some(difficulty.n_sliders as u32).into(),
                nSpinners: Some(difficulty.n_spinners as u32).into(),
                ar,
                cs,
                hp,
                od,
                bpm,
                clockRate: clock_rate,
                timePreempt: Some(hit_windows.ar).into(),
                greatHitWindow: Some(hit_windows.od).into(),
                EffectiveMissCount: Some(effective_miss_count).into(),
                ..Default::default()
            },
            PerformanceAttributes::Taiko(TaikoPerformanceAttributes {
                pp,
                pp_acc,
                pp_difficulty,
                difficulty,
                effective_miss_count,
            }) => Self {
                mode: Mode::Taiko,
                pp,
                ppAcc: Some(pp_acc).into(),
                ppDifficulty: Some(pp_difficulty).into(),
                stars: difficulty.stars,
                maxCombo: Some(difficulty.max_combo as u32).into(),
                nCircles: Some(map.n_circles).into(),
                nSliders: Some(map.n_sliders).into(),
                nSpinners: Some(map.n_spinners).into(),
                ar,
                cs,
                hp,
                od,
                bpm,
                clockRate: clock_rate,
                greatHitWindow: Some(hit_windows.od).into(),
                EffectiveMissCount: Some(effective_miss_count).into(),
                ..Default::default()
            },
        }
    }
}

impl std::fmt::Debug for CalculateResult {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        let mut s = f.debug_struct("CalculateResult");

        s.field("mode", &self.mode)
            .field("stars", &self.stars)
            .field("pp", &self.pp);

        if let Some(ref pp_acc) = self.ppAcc.into_option() {
            s.field("ppAcc", pp_acc);
        }

        if let Some(ref pp_aim) = self.ppAim.into_option() {
            s.field("ppAim", pp_aim);
        }

        if let Some(ref pp_flashlight) = self.ppFlashlight.into_option() {
            s.field("ppFlashlight", pp_flashlight);
        }

        if let Some(ref pp_speed) = self.ppSpeed.into_option() {
            s.field("ppSpeed", pp_speed);
        }

        if let Some(ref pp_strain) = self.ppStrain.into_option() {
            s.field("ppStrain", pp_strain);
        }

        if let Some(ref n_fruits) = self.nFruits.into_option() {
            s.field("nFruits", n_fruits);
        }

        if let Some(ref n_droplets) = self.nDroplets.into_option() {
            s.field("nDroplets", n_droplets);
        }

        if let Some(ref n_tiny_droplets) = self.nTinyDroplets.into_option() {
            s.field("nTinyDroplets", n_tiny_droplets);
        }

        if let Some(ref aim_strain) = self.aimStrain.into_option() {
            s.field("aimStrain", aim_strain);
        }

        if let Some(ref speed_strain) = self.speedStrain.into_option() {
            s.field("speedStrain", speed_strain);
        }

        if let Some(ref flashlight_rating) = self.flashlightRating.into_option() {
            s.field("flashlightRating", flashlight_rating);
        }

        if let Some(ref slider_factor) = self.sliderFactor.into_option() {
            s.field("sliderFactor", slider_factor);
        }

        s.field("ar", &self.ar)
            .field("cs", &self.cs)
            .field("hp", &self.hp)
            .field("od", &self.od)
            .field("bpm", &self.bpm)
            .field("clockRate", &self.clockRate);

        if let Some(ref time_preempt) = self.timePreempt.into_option() {
            s.field("timePreempt", time_preempt);
        }

        if let Some(ref great_hit_window) = self.greatHitWindow.into_option() {
            s.field("greatHitWindow", great_hit_window);
        }

        if let Some(ref n_circles) = self.nCircles.into_option() {
            s.field("nCircles", n_circles);
        }

        if let Some(ref n_sliders) = self.nSliders.into_option() {
            s.field("nSliders", n_sliders);
        }

        if let Some(ref n_spinners) = self.nSpinners.into_option() {
            s.field("nSpinners", n_spinners);
        }

        if let Some(ref combo) = self.maxCombo.into_option() {
            s.field("maxCombo", combo);
        }

        s.finish()
    }
}

#[ffi_function]
#[no_mangle]
pub extern "C" fn debug_result(res: &CalculateResult) {
    println!("{res:?}");
}
