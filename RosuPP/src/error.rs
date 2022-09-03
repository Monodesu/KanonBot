use interoptopus::ffi_type;

// This file may look complex but the Interoptopus parts are actually really simple,
// with some Rust best practices making up most of the code.

// This is the FFI error enum you want your users to see. You are free to name and implement this
// almost any way you want.
#[ffi_type(patterns(ffi_error))]
#[repr(C)]
pub enum FFIError {
    Ok = 0,
    Null = 100,
    Panic = 200,
    ParseError = 300,
    InvalidString = 400,
    Unknown = 1000,
}

// Implement Default so we know what the "good" case is.
impl Default for FFIError {
    fn default() -> Self {
        Self::Ok
    }
}

// Implement Interoptopus' `FFIError` trait for your FFIError enum.
// Here you must map 3 "well known" variants to your enum.
impl interoptopus::patterns::result::FFIError for FFIError {
    const SUCCESS: Self = Self::Ok;
    const NULL: Self = Self::Null;
    const PANIC: Self = Self::Panic;
}


use thiserror::Error;
#[derive(Error, Debug)]
pub enum Error {
    #[error("Unknown error")]
    Unknown,
    #[error("ParseError")]
    ParseError(#[from] rosu_pp::ParseError),
    #[error("Invalid String")]
    InvalidString(#[from] Option<std::str::Utf8Error>),
}

// Implement Default so we know what the "good" case is.
impl Default for Error {
    fn default() -> Self {
        Self::Unknown
    }
}

/// Provide a mapping how your Rust error enums translate
/// to your FFI error enums.
impl From<Error> for FFIError {
    fn from(x: Error) -> Self {
        match x {
            Error::Unknown => Self::Unknown,
            Error::ParseError(_) => Self::ParseError,
            Error::InvalidString(_) => Self::InvalidString,
        }
    }
}