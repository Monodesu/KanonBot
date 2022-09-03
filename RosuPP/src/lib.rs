use interoptopus::{extra_type, ffi_type, pattern, Inventory, InventoryBuilder};

mod calculator;
mod error;
mod params;
mod result;
use calculator::Calculator;
use error::{FFIError, Error};
use params::ScoreParams;
use result::CalculateResult;

#[ffi_type]
#[repr(C)]
#[derive(Copy, Clone, Debug, Hash, PartialEq, Eq)]
pub enum Mode {
    /// osu!standard
    Osu = 0,
    /// osu!taiko
    Taiko = 1,
    /// osu!catch
    Catch = 2,
    /// osu!mania
    Mania = 3,
}

impl Default for Mode {
    fn default() -> Self {
        Mode::Osu
    }
}

impl std::fmt::Display for Mode {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_fmt(format_args!("{}", self))
    }
}

// This will create a function `my_inventory` which can produce
// an abstract FFI representation (called `Library`) for this crate.
pub fn my_inventory() -> Inventory {
    InventoryBuilder::new()
        .register(pattern!(Calculator))
        .register(extra_type!(CalculateResult))
        .register(extra_type!(Mode))
        .register(pattern!(ScoreParams))
        .inventory()
}
