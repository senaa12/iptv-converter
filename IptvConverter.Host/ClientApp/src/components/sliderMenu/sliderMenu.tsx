import React, { CSSProperties, useCallback, useMemo } from 'react';
import classNames from 'classnames';

import './sliderMenu.scss';

interface SliderMenuProps {
    options: any[];
    selected: any;
    getLabelId: (option: any) => string | number;
    getLabel: (optionId: any) => string;
    onSelect: (optionId: any) => void;
    width: number | string;
    className?: string;
    style?: CSSProperties;
}

const sliderMenu = ({ options, getLabel, onSelect, getLabelId, width, selected, className, style }: SliderMenuProps) => {
    const barStyle = useMemo(
        () => { 
            const selectedIndex = options.findIndex(x => getLabelId(x) === getLabelId(selected));

            return {
                width: `calc(${width}${typeof(width) === 'number' ? 'px' : ''}/${options.length})`,
                left: `calc(${selectedIndex} * (${width}${typeof(width) === 'number' ? 'px' : ''}/${options.length}))`
            } as CSSProperties;
        },
        [selected, getLabelId, options]
    );

    const menuItemOnClick = useCallback(
        (newItem: any) => () => {
            if(getLabelId(selected) !== getLabelId(newItem)) {
                onSelect(newItem);
            }
        },
        [onSelect, getLabelId, selected]
    );

    const menuItemClassname = (itemType: any) => classNames('menu-item', 'clickable', {
        'active': getLabelId(itemType) === getLabelId(selected)
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
                            {getLabel(option)}
                        </div>
                    )
                })
            }
            <div className={'bar'} style={barStyle}/>  
        </div>
    )
}

export default sliderMenu;