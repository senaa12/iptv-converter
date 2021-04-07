import React, { CSSProperties, useCallback, useMemo } from 'react';
import classNames from 'classnames';

import './sliderMenu.scss';

interface SliderMenuProps {
    options: any[];
    selectedOption: any;
    getOptionId: (option: any) => string | number;
    getOptionLabel: (optionId: any) => string;
    onSelect: (optionId: any) => void;
    width: number | string;
    className?: string;
    style?: CSSProperties;
}

const sliderMenu = ({ options, getOptionId, onSelect, getOptionLabel, width, selectedOption, className, style }: SliderMenuProps) => {
    const barStyle = useMemo(
        () => { 
            const selectedIndex = options.findIndex(x => getOptionId(x) === getOptionId(selectedOption));

            return {
                width: `calc(${width}${typeof(width) === 'number' ? 'px' : ''}/${options.length})`,
                left: `calc(${selectedIndex} * (${width}${typeof(width) === 'number' ? 'px' : ''}/${options.length}))`
            } as CSSProperties;
        },
        [selectedOption, getOptionId, options]
    );

    const menuItemOnClick = useCallback(
        (newItem: any) => () => {
            if(getOptionId(selectedOption) !== getOptionId(newItem)) {
                onSelect(newItem);
            }
        },
        [onSelect, getOptionId, selectedOption]
    );

    const menuItemClassname = (itemType: any) => classNames('menu-item', {
        'active': getOptionId(itemType) === getOptionId(selectedOption)
    });

    const computedClassName = classNames('slider-menu', className, {
    });
    return (
        <div className={computedClassName} style={{ ...style, width: width }}>
            {
                options.map((option, index) => {

                    return (
                        <div 
                            key={index}
                            style={{ width: `calc(${width}${typeof(width) === 'number' ? 'px' : ''}/${options.length})` }}
                            onClick={menuItemOnClick(option)}
                            className={menuItemClassname(option)}
                        >
                            {getOptionLabel(option)}
                        </div>
                    )
                })
            }
            <div className={'bar'} style={barStyle}/>  
        </div>
    )
}

export default sliderMenu;