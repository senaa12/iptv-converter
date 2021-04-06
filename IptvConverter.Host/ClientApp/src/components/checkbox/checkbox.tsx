import classNames from 'classnames';
import React, { CSSProperties, useCallback } from 'react';
import Icon from '../icon/icon';
import IconEnum from '../icon/iconEnum';

import './checkbox.scss';

interface CheckboxProps {
    checkboxClassname?: string;
    labelClassname?: string;
    disabled?: boolean;
    label?: React.ReactNode;
    checked?: boolean;
    handleOnCheckboxChange?: (value: any) => void;
    style?: CSSProperties;
}

const Checkbox = ({
    checked, disabled, handleOnCheckboxChange, label, labelClassname, checkboxClassname, style
}: CheckboxProps) => {
    const checkboxOnClick = useCallback(() => {
        if (!disabled && handleOnCheckboxChange) {
            handleOnCheckboxChange(!checked);
        }
    }, [checked, disabled, handleOnCheckboxChange]);

    const checkboxclassname = classNames('checkbox-base', checkboxClassname);
    const labelclassname = classNames('label-base', labelClassname, {
        checked,
        disabled,
    });

    return (
        <div
            className={checkboxclassname}
            style={style}
        >
            <label className={labelclassname} onClick={checkboxOnClick}>
                {checked && <div className={'icon-span'}>
                    <Icon iconName={IconEnum.Check} height={14} width={14} />
				</div>}
				<span className={'span'}>{label}</span>
			</label>
        </div>
    );
};

export default Checkbox;
